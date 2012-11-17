﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Routing;
using System.Web.Mvc;
using System.Web;
using System.Reflection;

namespace Lib.Web.Mvc.JQuery.JqGrid.DataAnnotations
{
    /// <summary>
    /// Base class which specifies the options for jqGrid editable or searchable column element
    /// </summary>
    public abstract class JqGridColumnElementAttribute : Attribute, IMetadataAware
    {
        #region Properties
        internal string DataUrl
        {
            get
            {
                if ((HttpContext.Current != null) && (!String.IsNullOrWhiteSpace(DataUrlRouteName) || DataUrlRouteData != null))
                {
                    RouteValueDictionary dataUrlRouteValueDictionary = DataUrlRouteData;
                    if (DataUrlRouteValues != null)
                    {
                        dataUrlRouteValueDictionary = new RouteValueDictionary(DataUrlRouteValues);
                        if (DataUrlRouteData != null)
                        {
                            foreach (string key in DataUrlRouteData.Keys)
                            {
                                if (dataUrlRouteValueDictionary.ContainsKey(key))
                                    dataUrlRouteValueDictionary[key] = DataUrlRouteData[key];
                                else
                                    dataUrlRouteValueDictionary.Add(key, DataUrlRouteData[key]);
                            }
                        }
                    }
                    VirtualPathData dataUrlPathData = RouteTable.Routes.GetVirtualPathForArea(HttpContext.Current.Request != null ? HttpContext.Current.Request.RequestContext : null, DataUrlRouteName, dataUrlRouteValueDictionary);

                    if (dataUrlPathData == null)
                        throw new InvalidOperationException("The DataUrl could not be resolved.");

                    return dataUrlPathData.VirtualPath;
                }
                return null;
            }
        }

        internal RouteValueDictionary DataUrlRouteData { get; set; }

        internal string DataUrlRouteName { get; set; }

        /// <summary>
        /// When overriden in delivered class, provides additional route values for the select element AJAX request.
        /// </summary>
        protected virtual object DataUrlRouteValues
        {
            get { return null; }
        }

        /// <summary>
        /// Gets or sets the function which will build the select element in case where the server response can not build it (requires DataUrl property to be set).
        /// </summary>
        public string BuildSelect
        {
            get { return Options.BuildSelect; }
            set { Options.BuildSelect = value; }
        }

        /// <summary>
        /// Gets or sets if the value should be validated with custom function.
        /// </summary>
        public bool CustomValidation
        {
            get { return Rules.Custom; }
            set { Rules.Custom = value; }
        }

        /// <summary>
        /// Gets or sets the name of custom validation function
        /// </summary>
        public string CustomValidationFunction
        {
            get { return Rules.CustomFunction; }
            set { Rules.CustomFunction = value; }
        }

        /// <summary>
        /// When overriden in delivered class, provides a list of events to apply to the element.
        /// </summary>
        protected virtual IList<JqGridColumnDataEvent> DataEvents
        {
            get { return null; }
        }

        /// <summary>
        /// Gets or sets the function which will be called once when the element is created.
        /// </summary>
        public string DataInit
        {
            get { return Options.DataInit; }
            set { Options.DataInit = value; }
        }

        /// <summary>
        /// Gets or sets if the value should be valid date.
        /// </summary>
        public bool DateValidation
        {
            get { return Rules.Date; }
            set { Rules.Date = value; }
        }

        /// <summary>
        /// Gets or sets the default value in the search input element.
        /// </summary>
        public string DefaultValue
        {
            get { return Options.DefaultValue; }
            set { Options.DefaultValue = value; }
        }

        /// <summary>
        /// Gets or sets if the value should be valid email
        /// </summary>
        public bool EmailValidation
        {
            get { return Rules.Email; }
            set { Rules.Email = value; }
        }

        /// <summary>
        /// When overriden in delivered class, provides a dictionary where keys should be valid attributes for the element.
        /// </summary>
        protected virtual IDictionary<string, object> HtmlAttributes
        {
            get { return null; }
        }

        internal JqGridColumnElementOptions Options { get; set; }

        internal JqGridColumnRules Rules { get; set; }

        /// <summary>
        /// Gets or sets if the value should be valid time (hh:mm format and optional am/pm at the end).
        /// </summary>
        public bool TimeValidation
        {
            get { return Rules.Time; }
            set { Rules.Time = value; }
        }

        /// <summary>
        /// Gets or sets if the value should be valid url.
        /// </summary>
        public bool UrlValidation
        {
            get { return Rules.Url; }
            set { Rules.Url = value; }
        }

        /// <summary>
        /// Gets or sets the set of value:label pairs for select element.
        /// </summary>
        public virtual string Value{ get; set; }

        internal Type ValueProviderType { get; set; }

        internal string ValueProviderMethodName { get; set; }

        internal IDictionary<string, string> ValueDictionary
        {
            get
            {
                if (ValueProviderType != null && !String.IsNullOrWhiteSpace(ValueProviderMethodName))
                {
                    MethodInfo valueProviderMethodInfo = ValueProviderType.GetMethod(ValueProviderMethodName);
                    if (valueProviderMethodInfo == null)
                        throw new InvalidOperationException("The method specified by ValueProviderType and ValueProviderMethodName could not be found.");

                    ConstructorInfo valueProviderConstructorInfo = ValueProviderType.GetConstructor(Type.EmptyTypes);
                    if (valueProviderConstructorInfo == null)
                        throw new InvalidOperationException("The type specified by ValueProviderType does not have parameterless constructor.");

                    object valueProvider = valueProviderConstructorInfo.Invoke(null);
                    if (typeof(IDictionary<string, string>).IsAssignableFrom(valueProviderMethodInfo.ReturnType))
                        return (IDictionary<string, string>)valueProviderMethodInfo.Invoke(valueProvider, null);
                    else
                        throw new InvalidOperationException("The method specified by ValueProviderType and ValueProviderMethodName does not return IDictionary<string, string>.");
                }
                return null;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the JqGridColumnElementAttribute class.
        /// </summary>
        public JqGridColumnElementAttribute()
        {
            Rules = new JqGridColumnRules();
        }
        #endregion

        #region IMetadataAware
        /// <summary>
        /// Provides metadata to the model metadata creation process.
        /// </summary>
        /// <param name="metadata">The model metadata.</param>
        public void OnMetadataCreated(ModelMetadata metadata)
        {
            Options.Value = Value;

            if ((metadata.ModelType == typeof(Int16)) || (metadata.ModelType == typeof(Int32)) || (metadata.ModelType == typeof(Int64)) || (metadata.ModelType == typeof(UInt16)) || (metadata.ModelType == typeof(UInt32)) || (metadata.ModelType == typeof(UInt32)))
                Rules.Integer = true;
            else if ((metadata.ModelType == typeof(Decimal)) || (metadata.ModelType == typeof(Double)) || (metadata.ModelType == typeof(Single)))
                Rules.Number = true;

            InternalOnMetadataCreated(metadata);
        }

        /// <summary>
        /// Provides metadata to the model metadata creation process in derivered class.
        /// </summary>
        /// <param name="metadata">The model metadata.</param>
        protected abstract void InternalOnMetadataCreated(ModelMetadata metadata);
        #endregion
    }
}
