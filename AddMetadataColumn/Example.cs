using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using Tridion.ContentManager;  // C:\Program Files (x86)\Tridion\bin\client
using Tridion.Web.UI.Core.Extensibility;  // C:\Program Files (x86)\Tridion\web\WebUI\WebRoot\bin

// code samples used:
// http://www.sdltridionworld.com/community/2011_extensions/parentchangenotifier.aspx
// http://sdllivecontent.sdl.com/LiveContent/web/pub.xql?action=home&pub=SDL_Tridion_2011_SPONE&lang=en-US#addHistory=true&filename=AddingANewColumnToAListView.xml&docid=task_E4EFBE6E5CA24C01B2531FB15AE95AE2&inner_id=&tid=&query=&scope=&resource=&eventType=lcContent.loadDoctask_E4EFBE6E5CA24C01B2531FB15AE95AE2


namespace GuiExtensions.DataExtenders
{
    public class Example : DataExtender
    {
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
            if (command == "GetList")
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

        private XmlTextReader PreprocessListItems(XmlTextReader xReader, PipelineContext context)
        {
            TextWriter sWriter = new StringWriter();
            XmlTextWriter xWriter = new XmlTextWriter(sWriter);
            string attrName = "metadataFieldValue";  // Must be same as extension config,  'selector="@metadataFieldValue"'
            string attrValue = "fieldValue";  // default here to confirm GUI DataExtender is working - will update value below

            xReader.MoveToContent();
            
            while (!xReader.EOF)
            {
                switch (xReader.NodeType)
                {
                    case XmlNodeType.Element:
                        xWriter.WriteStartElement(xReader.Prefix, xReader.LocalName, xReader.NamespaceURI);

                        // add all list attributes back  -- always START with this to NOT break the GUI
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
                                    // Do Work Here....
                                    attrValue = "your new value";
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
            
            xWriter.Flush();

            xReader = new XmlTextReader(new StringReader(sWriter.ToString()));
            xReader.MoveToContent();

            //-> Write XML of tcm:Item out...
            //   This is where the attribute in the config file is matched -> selector="@metadataFieldValue"
            // Trace.Write(sWriter.ToString() + Environment.NewLine);  
            return xReader;
        }

        private bool IsValidItem(XmlTextReader xReader)
        {
            if (xReader.LocalName == "Item")// && xReader.NamespaceURI == TDSDefines.Constants.NS_DS)
                return true;
            else
                return false;
        }
    }
}