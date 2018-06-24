using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Serialization;

namespace MSOWeb
{
    public class XmlResult : ActionResult
    {
        private object _objectToSerialize;
        private readonly bool _suppressNamespaces;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlResult"/> class.
        /// </summary>
        /// <param name="objectToSerialize">The object to serialize to XML.</param>
        public XmlResult(object objectToSerialize, bool suppressNamespaces = false)
        {
            _objectToSerialize = objectToSerialize;
            _suppressNamespaces = suppressNamespaces;
        }

        /// <summary>
        /// Gets the object to be serialized to XML.
        /// </summary>
        public object ObjectToSerialize
        {
            get { return _objectToSerialize; }
        }

        /// <summary>
        /// Serialises the object that was passed into the constructor to XML and writes the corresponding XML to the result stream.
        /// </summary>
        /// <param name="context">The controller context for the current request.</param>
        public override void ExecuteResult(ControllerContext context)
        {
            if (_objectToSerialize != null)
            {
                context.HttpContext.Response.Clear();
                context.HttpContext.Response.ContentType = "text/xml";

                var xs = new XmlSerializer(_objectToSerialize.GetType());
                if (_suppressNamespaces)
                {
                    XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                    ns.Add("", "");
                    xs.Serialize(context.HttpContext.Response.Output, _objectToSerialize, ns);
                }
                else
                    xs.Serialize(context.HttpContext.Response.Output, _objectToSerialize);
            }
        }
    }
}