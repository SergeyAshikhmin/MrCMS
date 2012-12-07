﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Mvc;
using MrCMS.Entities.Documents.Layout;
using MrCMS.Entities.Documents.Web;
using NHibernate;
using MrCMS.Helpers;

namespace MrCMS.Entities.Widget
{
    public abstract class Widget : BaseEntity
    {
        public virtual LayoutArea LayoutArea { get; set; }

        public virtual string Name { get; set; }

        public virtual string WidgetType { get { return GetType().Name; } }
        public virtual string WidgetTypeFormatted { get { return WidgetType.BreakUpString(); } }

        public virtual Webpage Webpage { get; set; }
        public virtual int DisplayOrder { get; set; }

        [DefaultValue(true)]
        [DisplayName("Is Recursive")]
        public virtual bool IsRecursive { get; set; }

        public virtual IList<PageWidgetSort> PageWidgetSorts { get; set; }

        [DisplayName("Custom Layout (leave blank to use default)")]
        public virtual string CustomLayout { get; set; }

        public virtual object GetModel(ISession session)
        {
            return this;
        }

        public virtual IList<Webpage> HiddenOn { get; set; }
        public virtual IList<Webpage> ShownOn { get; set; }

        public virtual void SetDropdownData(ViewDataDictionary viewData, ISession session) { }

        public virtual bool HasProperties { get { return true; } }

        public override void OnDeleting()
        {
            ShownOn.ForEach(webpage => webpage.ShownWidgets.Remove(this));
            HiddenOn.ForEach(webpage => webpage.HiddenWidgets.Remove(this));
            LayoutArea.Widgets.Remove(this); //required to clear cache
            if (Webpage != null)
            {
                Webpage.Widgets.Remove(this);
            }
            base.OnDeleting();
        }
    }
}