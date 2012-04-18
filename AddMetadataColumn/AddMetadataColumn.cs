using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using AddMetadataColumn.CoreService;
using GuiExtensions.DataExtenders.Utils.CoreServiceHandler;
using Tridion.ContentManager;
using Tridion.ContentManager.Interop.TDS;
using Tridion.Web.UI.Core.Extensibility;
using Tridion.Web.UI.Models.TCM54;

// code samples used:
// http://sdllivecontent.sdl.com/LiveContent/web/pub.xql?action=home&pub=SDL_Tridion_2011_SPONE&lang=en-US#addHistory=true&filename=AddingANewColumnToAListView.xml&docid=task_E4EFBE6E5CA24C01B2531FB15AE95AE2&inner_id=&tid=&query=&scope=&resource=&eventType=lcContent.loadDoctask_E4EFBE6E5CA24C01B2531FB15AE95AE2
// http://www.sdltridionworld.com/community/2011_extensions/parentchangenotifier.aspx
// http://jaimesantosalcon.blogspot.com/2012/04/sdl-tridion-2011-data-extenders-real.html

namespace GuiExtensions.DataExtenders 
{
    public class AddMetadataColumn : DataExtender
    {
        const string _metadataFieldname = "article_number";
        static CoreServiceHandler _coreServiceHandler;
        static XmlDocument _xmlDoc;

        public override string Name
        {
            get
            {
                Type itsMe = this.GetType();
                return String.Concat(itsMe.Namespace, ".", itsMe.Name);
            }
        }

        public override XmlTextReader ProcessRequest(XmlTextReader reader, PipelineContext context)
        {
            return reader;
        }

        public override XmlTextReader ProcessResponse(XmlTextReader reader, PipelineContext context)
        {
            XmlTextReader xReader = reader;
            string command = context.Parameters["command"] as String;
            if (command == "GetList") // Code runs on every GetList
            {
                try
                {
                    Trace.Write("==========================Start PreprocessListItems " + System.DateTime.Now.ToShortDateString() + ", " + System.DateTime.Now.ToLongTimeString() + Environment.NewLine);
                    xReader = PreprocessListItems(reader, context);
                    Trace.Write("==========================Stop PreprocessListItems " + System.DateTime.Now.ToShortDateString() + ", " + System.DateTime.Now.ToLongTimeString() + Environment.NewLine);
                }
                catch
                { }
            }
            return xReader;
        }

        /// <summary>
        /// Idea here is to re-create the XmlTextReader Node and this accounts for 50% of the code.
        /// Original code borrowed from http://www.sdltridionworld.com/community/2011_extensions/parentchangenotifier.aspx
        /// Thanks for the work from Serguei Martchenko - would not be possible without his example!
        /// </summary>
        /// <param name="xReader"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private XmlTextReader PreprocessListItems(XmlTextReader xReader, PipelineContext context)
        {
            TextWriter sWriter = new StringWriter();
            XmlTextWriter xWriter = new XmlTextWriter(sWriter);
            string attrName = "metadataFieldValue";
            string attrValue = "";  // set this to 'fieldValue', for example, to debug and prove it is working
 
            xReader.MoveToContent();
           
            using(var tdse = new TDSEWrapper())
            {
                while (!xReader.EOF)
                {
                    switch (xReader.NodeType)
                    {
                        case XmlNodeType.Element:
                            xWriter.WriteStartElement(xReader.Prefix, xReader.LocalName, xReader.NamespaceURI);
                           
                            // add all attributes back  -- always START with this to NOT break the GUI
                            xWriter.WriteAttributes(xReader, false);
                        
                            try
                            {
                                // add my custom attribute
                                if (IsValidItem(xReader))
                                {
                                    string id = xReader.GetAttribute("ID");  // URI
                                    TcmUri uri = new TcmUri(id);

                                    if (uri.ItemType == ItemType.Component)
                                    {
                                        // Get Metadata Value
                                        // 0 seconds with no processing
                                        // 12 seconds for 250 Components with TOM API, Component.MetadataFields...
                                        // 12 seconds for 250 Components with Core Service
                                        // 2 seconds for 250 Components with TOM API and GetXML  (FASTEST!)

                                        // Core Service
                                        //attrValue = GetMetadataValue(fieldName, id);

                                        // TDSE...Fastest with GetXML
                                        Component comp = tdse.TDSE.GetObject(id, Tridion.ContentManager.Interop.TDSDefines.EnumOpenMode.OpenModeView) as Component;
                                        attrValue = GetMetadataValue(comp, "article_number");
                                    }

                                    // add new metadata field attribute
                                    xWriter.WriteAttributeString(attrName, attrValue);
                                    xReader.MoveToElement();
                                }
                            }
                            catch (Exception ex)
                            {
                                Trace.TraceError("EXCEPTION " + ex.Message + ex.ToString() + ex.StackTrace);
                            }

                            if (xReader.IsEmptyElement)
                            {
                                xWriter.WriteEndElement();
                            }
                            break;

                        case XmlNodeType.EndElement:
                            xWriter.WriteEndElement();
                            break;
                        case XmlNodeType.CDATA:
                            // Copy CDATA node  <![CDATA[]]>
                            xWriter.WriteCData(xReader.Value);
                            break;
                        case XmlNodeType.Comment:
                            // Copy comment node <!-- -->
                            xWriter.WriteComment(xReader.Value);
                            break;
                        case XmlNodeType.DocumentType:
                            // Copy XML documenttype
                            xWriter.WriteDocType(xReader.Name, null, null, null);
                            break;
                        case XmlNodeType.EntityReference:
                            xWriter.WriteEntityRef(xReader.Name);
                            break;
                        case XmlNodeType.ProcessingInstruction:
                            xWriter.WriteProcessingInstruction(xReader.Name, xReader.Value);
                            break;
                        case XmlNodeType.SignificantWhitespace:
                            xWriter.WriteWhitespace(xReader.Value);
                            break;
                        case XmlNodeType.Text:
                            xWriter.WriteString(xReader.Value);
                            break;
                        case XmlNodeType.Whitespace:
                            xWriter.WriteWhitespace(xReader.Value);
                            break;
                    }
                    xReader.Read();
                };
            }
            xWriter.Flush();

            xReader = new XmlTextReader(new StringReader(sWriter.ToString()));
            xReader.MoveToContent();
            //-> Write XML of tcm:Item out...
            //   This is where the attribute in the config file is matched.  
            // Trace.Write(sWriter.ToString() + Environment.NewLine);  
            return xReader;
        }

        /// <summary>
        /// Uses TOM TDSE and not Core Service.
        /// TOM XML Method is fastest 
        /// </summary>
        /// <param name="comp">Component</param>
        /// <param name="fieldname">Metadata fieldname</param>
        /// <returns>Value of Metadata field</returns>
        private string GetMetadataValue(Component comp, string fieldname)
        {
            // TOM API - slow 12 seconds
            //if (comp.MetadataFields.Count > 0)
            //{
            //    if (comp.MetadataFields[fieldname] != null)
            //    {
            //        attrValue = comp.MetadataFields[fieldname].value[1];
            //    }
            //}

            // TOM XML - Fastest, only 2 seconds
            string value = "";
            XmlDocument xmlDoc = GetXmlDoc();
            xmlDoc.LoadXml(comp.GetXML(Tridion.ContentManager.Interop.TDSDefines.XMLReadFilter.XMLReadDataContent));
            string xPath = String.Format("//*[local-name()='{0}']", fieldname);
            if (xmlDoc.SelectSingleNode(xPath) != null)
            {
                value = xmlDoc.SelectSingleNode(xPath).InnerText;
            }

            return value;
        }

        /// <summary>
        /// Implementation using Core Service
        /// </summary>
        /// <param name="fieldname">Metadata fieldname</param>
        /// <param name="uri">Component URI</param>
        /// <returns></returns>
        public static string GetMetadataValue(string fieldname, string uri)
        {
            string value = "";
            ComponentData compData = GetComponentData(uri);
            XmlDocument xmlDoc = GetXmlDoc();  // should we get a new one each time or re-use a global one?
            xmlDoc.LoadXml(compData.Metadata);
            
            //Trace.Write(Environment.NewLine + "===== XML->" + metadata + Environment.NewLine);
            
            string xPath = String.Format("//*[local-name()='{0}']", fieldname);
            if (xmlDoc.SelectSingleNode(xPath) != null)
            {
                value = xmlDoc.SelectSingleNode(xPath).InnerText;
            }
            return value;
        }

        /// <summary>
        /// Get Xml Doc
        /// </summary>
        /// <returns>XmlDocument</returns>
        private static XmlDocument GetXmlDoc()
        {
            if (_xmlDoc == null)
            {
                XmlDocument xmlDoc = new XmlDocument();
                _xmlDoc = xmlDoc;
            }
            return _xmlDoc;
        }

        /// <summary>
        /// Read ComponentData from the Core Service, via the CoreServiceHandler
        /// </summary>
        /// <param name="uri">URI of Component</param>
        /// <returns>ComponentData</returns>
        private static ComponentData GetComponentData(string uri)
        {
            CoreServiceHandler coreServiceHandler = GetCoreServiceHandler();
            ICoreService2010 core = coreServiceHandler.GetNewClient();
            ComponentData compData = core.Read(uri, new ReadOptions()) as ComponentData;
            return compData;
        }

        /// <summary>
        /// Get connection to Core Service - expecting the values to be in a CoreServiceHandler.config file.
        /// </summary>
        /// <returns>Core Service Handler</returns>
        private static CoreServiceHandler GetCoreServiceHandler()
        {
            if (_coreServiceHandler == null)
            {
                try
                {
                    string configFilename = "CoreServiceHandler.config";
                    _coreServiceHandler = new CoreServiceHandler(configFilename);
                }
                catch (Exception ex)
                {
                    Trace.Write("CORE SERVICE ERROR - Cannot get core service" + Environment.NewLine);
                }
            }

            return _coreServiceHandler;
        }

        /// <summary>
        /// Check if an item node 
        /// </summary>
        /// <param name="xReader"></param>
        /// <returns>True if we have an Item node</returns>
        private bool IsValidItem(XmlTextReader xReader)
        {
            if (xReader.LocalName == "Item")// && xReader.NamespaceURI == TDSDefines.Constants.NS_DS)
                return true;
            else
                return false;
        }
    }
}
