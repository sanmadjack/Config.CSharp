using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using MVC;
using Exceptions;
namespace Config {

    public class XmlSettingsFile : ASettingsSource {

        private XmlDocument config;


        public XmlSettingsFile(string app_name, ConfigMode mode)
            : base(app_name,mode) {
                this.file_extension = "xml";
        }


        protected override bool createConfig(string file_name) {
            XmlTextWriter write_here = new XmlTextWriter(file_name, System.Text.Encoding.UTF8);
            write_here.Formatting = Formatting.Indented;
            write_here.WriteProcessingInstruction("xml", "version='1.0' encoding='UTF-8'");
            write_here.WriteStartElement("config");
            write_here.Close();
            return true;
        }

        protected override bool loadConfig(FileStream stream) {
            XmlReaderSettings xml_settings = new XmlReaderSettings();
            xml_settings.ConformanceLevel = ConformanceLevel.Document;
            xml_settings.IgnoreComments = true;
            xml_settings.IgnoreWhitespace = true;
            config = new XmlDocument();

            XmlReader reader = XmlReader.Create(stream, xml_settings);
            try {
                config.Load(reader);
            } catch (XmlException e) {
                if (e.Message.StartsWith("Root element is missing")) {
                    reader.Close();
                    stream.Close();
                    createConfig(file_full_path);
                    stream = new FileStream(file_full_path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    reader = XmlReader.Create(stream, xml_settings);
                    config.Load(reader);
                } else {
                    throw e;
                }
            } finally {
                reader.Close();
            }
            return true;

        }

        protected override bool writeConfig(FileStream stream) {
            lock (config) {
                config.Save(stream);
            }
            return true;
        }

        private XmlElement getNode(bool create, params string[] children) {
            if (!config.HasChildNodes)
                return null;

            XmlElement nodes = config.ChildNodes[1] as XmlElement;
            foreach (string child in children) {
                bool found = false;
                foreach (XmlElement node in nodes.ChildNodes) {
                    if (node.Name == child) {
                        nodes = node;
                        found = true;
                        break;
                    }
                }
                if (!found) {
                    if (create) {
                        XmlElement new_node = config.CreateElement(child);
                        nodes.AppendChild(new_node);
                        nodes = new_node;
                    } else {
                        nodes = null;
                        break;
                    }
                }
            }
            return nodes;
        }



        protected List<string> getNodeGroupValues(string name, params string[] names) {
            List<string> return_me = new List<string>();
            ;
            lock (config) {
                XmlElement node = getNode(false, names);
                if (node == null)
                    return return_me;
                foreach (XmlElement child in node.ChildNodes) {
                    if (child.Name == name)
                        return_me.Add(child.InnerText);
                }
            }
            return return_me;
        }
        protected bool setNodeGroupValues(string name, List<string> values, params string[] names) {
            lock (config) {
                XmlElement node = getNode(false, names);
                if (node != null)
                    node.ParentNode.RemoveChild(node);

                node = getNode(true, names);

                foreach (string value in values) {
                    XmlElement element = config.CreateElement(name);
                    element.InnerText = value;
                    node.AppendChild(element);
                }
            }
            return writeConfig();
        }
        protected string getNodeValue(params string[] names) {
            lock (config) {
                XmlElement node = getNode(false, names);
                if (node == null)
                    return null;
                return node.InnerText;
            }
        }
        protected string getNodeAttribute(string attribute, params string[] name) {
            lock (config) {
                XmlElement node = getNode(false, name);
                if (node == null)
                    return null;

                if (!node.HasAttribute(attribute))
                    return null;

                return node.GetAttribute(attribute);
            }
        }

        protected bool setNodeValue(string value, params string[] name) {
            lock (config) {
                XmlElement node = getNode(true, name);
                if (node == null)
                    return false;
                node.InnerText = value;
            }
            return writeConfig();
        }
        protected bool setNodeAttribute(string attribute, string value, params string[] name) {
            lock (config) {
                XmlElement node = getNode(true, name);
                if (node == null)
                    return false;
                node.SetAttribute(attribute, value);
            }
            return writeConfig();
        }
        protected XmlElement getSpecificNode(string name, bool create, params string[] attribs) {
            if (!config.HasChildNodes)
                return null;

            if (attribs.Length % 2 != 0)
                throw new Exception("An uneven number of identifying attribute values has been supplied");

            XmlElement return_me = null;
            XmlElement nodes = config.ChildNodes[1] as XmlElement;
            foreach (XmlElement node in nodes.ChildNodes) {
                if (node.Name != name)
                    continue;

                return_me = node;
                for (int i = 0; i < attribs.Length; i++) {
                    if (!node.HasAttribute(attribs[i])) {
                        return_me = null;
                        break;
                    }
                    string attrib = node.GetAttribute(attribs[i]);
                    i++;
                    if (!attrib.Equals(attribs[i])) {
                        return_me = null;
                        break;
                    }
                }
                if (return_me != null)
                    break;
            }
            if (return_me == null) {
                if (create) {
                    return_me = config.CreateElement(name);
                    for (int i = 0; i < attribs.Length; i++) {
                        return_me.SetAttribute(attribs[i], attribs[++i]);
                    }
                    nodes.AppendChild(return_me);
                }
            }
            return return_me;
        }
        protected bool setSpecificNodeValue(string name, string value, params string[] attribs) {
            lock (config) {
                XmlElement node = getSpecificNode(name, true, attribs);
                if (node == null)
                    return false;
                node.InnerText = value;
            }
            return writeConfig();
        }
        protected bool setSpecificNodeAttrib(string name, string attrib_name, string attrib_value, params string[] attribs) {
            lock (config) {
                XmlElement node = getSpecificNode(name, true, attribs);
                if (node == null)
                    return false;
                node.SetAttribute(attrib_name, attrib_value);
            }
            return writeConfig();
        }
        protected string getSpecificNodeValue(string name, params string[] attribs) {
            lock (config) {
                XmlElement node = getSpecificNode(name, false, attribs);
                if (node == null)
                    return null;
                return node.InnerText;
            }
        }
        protected string getSpecificNodeAttribute(string name, string attrib, params string[] attribs) {
            lock (config) {
                XmlElement node = getSpecificNode(name, false, attribs);
                if (node == null)
                    return null;

                if (!node.HasAttribute(attrib))
                    return null;

                return node.GetAttribute(attrib);
            }
        }

        protected bool clearNode(params string[] names) {
            lock (config) {
                XmlElement node = getNode(false, names);
                if (node == null)
                    return true;

                node = node.ParentNode as XmlElement;
                string name = names[names.Length - 1];
                for (int i = 0; i < node.ChildNodes.Count; i++) {
                    if (node.ChildNodes[i].Name == name) {
                        node.RemoveChild(node.ChildNodes[i]);
                        i--;
                    }
                }


            }
            return writeConfig();
        }
        protected bool clearSpecificNodeAttribute(string name, string attrib, params string[] attribs) {
            lock (config) {
                XmlElement node = getSpecificNode(name, false, attribs);
                if (node == null)
                    return false;

                if (!node.HasAttribute(attrib))
                    return false;

                node.RemoveAttribute(attrib);

                return writeConfig();
            }
        }
    }
}
