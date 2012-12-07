﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MrCMS.Entities.Multisite;
using MrCMS.Entities.Settings;
using MrCMS.Helpers;

namespace MrCMS.Settings
{
    public class ConfigurationProvider : IConfigurationProvider
    {
        private readonly ISettingService _settingService;

        public ConfigurationProvider(ISettingService settingService)
        {
            _settingService = settingService;
        }


        public void SaveSettings(ISettings settings)
        {
            var type = settings.GetType();
            IEnumerable<PropertyInfo> properties = from prop in type.GetProperties()
                                                   where prop.CanWrite && prop.CanRead
                                                   where prop.Name != "Site"
                                                   where
                                                       prop.PropertyType.GetCustomTypeConverter()
                                                           .CanConvertFrom(typeof(string))
                                                   select prop;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            foreach (PropertyInfo prop in properties)
            {
                string key = type.FullName + "." + prop.Name;
                //Duck typing is not supported in C#. That's why we're using dynamic type
                dynamic value = prop.GetValue(settings, null);
                if (value != null)
                    _settingService.SetSetting(settings.Site, key, value);
                else
                    _settingService.SetSetting(settings.Site, key, "");
            }
        }

        public void DeleteSettings(Site site, ISettings settings)
        {
            var type = settings.GetType();
            IEnumerable<PropertyInfo> properties = from prop in type.GetProperties()
                                                   select prop;

            List<Setting> settingList =
                properties.Select(prop => type.FullName + "." + prop.Name)
                          .Select(key => _settingService.GetSettingByKey(site, key))
                          .Where(setting => setting != null).ToList();

            foreach (Setting setting in settingList)
                _settingService.DeleteSetting(setting);
        }

        public List<ISettings> GetAllISettings(Site site)
        {
            var methodInfo = GetType().GetMethodExt("GetSettings", typeof (Site));

            return TypeHelper.GetAllConcreteTypesAssignableFrom<ISettings>()
                             .Select(type => methodInfo.MakeGenericMethod(type).Invoke(this, new object[] {site}))
                             .OfType<ISettings>().ToList();

        }

        public TSettings GetSettings<TSettings>(Site currentSite) where TSettings : ISettings, new()
        {
            var settings = Activator.CreateInstance<TSettings>();

            // get properties we can write to
            var properties = from prop in typeof(TSettings).GetProperties()
                             where prop.CanWrite && prop.CanRead
                             where prop.Name != "Site"
                             let setting =
                                 _settingService.GetSettingValueByKey<string>(currentSite,
                                 string.Format("{0}.{1}", typeof(TSettings).FullName, prop.Name))
                             where setting != null
                             where prop.PropertyType.GetCustomTypeConverter().CanConvertFrom(typeof(string))
                             where prop.PropertyType.GetCustomTypeConverter().IsValid(setting)
                             let value = prop.PropertyType.GetCustomTypeConverter().ConvertFromInvariantString(setting)
                             select new { prop, value };

            // assign properties
            properties.ToList().ForEach(p => p.prop.SetValue(settings, p.value, null));
            settings.Site = currentSite;

            return settings;
        }
    }
}