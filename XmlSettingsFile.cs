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
            : base(app_name,mode,"xml") {
        }


        public override List<string> read(Setting setting) {
            List<string> strings = new List<string>();
            List<XmlElement> nodes = getNodes(setting.path, false);
            foreach (XmlElement node in nodes) {
                strings.Add(node.InnerText);
            }
            return strings;
        }

        public override List<string> erase(Setting setting) {
            List<string> strings = new List<string>();
            List<XmlElement> nodes = getNodes(setting.path, false);
            while(nodes.Count > 0) {
                XmlElement node = nodes[0];
                strings.Add(node.InnerText);
                node.ParentNode.RemoveChild(node);
                nodes.Remove(node);
            }
            return strings;
        }

        public override bool erase(Setting setting, string value) {
            bool found = false;
            List<XmlElement> nodes = getNodes(setting.path, false);
            while (nodes.Count > 0) {
                XmlElement node = nodes[0];
                if (node.InnerText == value) {
                    node.ParentNode.RemoveChild(node);
                    found = true;
                }
                nodes.Remove(node);
            }
            return found;
        }
        public override void write(Setting setting, string value) {
            List<string> path = new List<string>(setting.path);
            string name = path[path.Count-1];
            path.RemoveAt(path.Count - 1);

            XmlElement node = getNodes(path, true)[0];

            XmlElement new_node = config.CreateElement(name);
            new_node.InnerText = value;
            node.AppendChild(new_node);
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

        private XmlElement ConfigNode {
            get {
                if (!config.HasChildNodes)
                    return null;
                return config.ChildNodes[1] as XmlElement;
            }
        }

        private List<XmlElement> getNodes(List<string> path, bool create) {
            List<XmlElement> return_me = new List<XmlElement>();
            if (config.HasChildNodes) {
                XmlElement node = ConfigNode;
                foreach(string name in path) {
                    bool found = false;
                    foreach (XmlElement child in node.ChildNodes) {
                        if (child.Name == name) {
                            found = true;
                            if (path[path.Count-1] == name) {
                                return_me.Add(child);
                            } else {
                                node = child;
                                break;
                            }
                        }
                    }
                    if (!found) {
                        if (create) {
                            XmlElement new_node = config.CreateElement(name);
                            node.AppendChild(new_node);
                            if (path[path.Count - 1] == name) {
                                return_me.Add(new_node);
                                break;
                            } else {
                                node = new_node;
                            }
                        } else {
                            break;
                        }
                    }
                }
            }
            return return_me;
        }


        private XmlElement getNode(bool create, List<string> children) {
            if (!config.HasChildNodes)
                return null;

            XmlElement nodes = ConfigNode;

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



    }
}
