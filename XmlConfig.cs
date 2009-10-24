using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Collections;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.IO;
using System.Linq;

namespace CommitMonkey {
	[XmlRoot("CommitMonkey")]
	public struct XmlConfig {
		public struct Project {
			[XmlAttribute("path")] public string Path;
		}
		[XmlElement("project")] public Project[] Projects;
	}
}
