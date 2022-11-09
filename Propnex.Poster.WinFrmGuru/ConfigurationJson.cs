using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Propnex.Poster.Guru
{
    public class ConfigurationJson<T>
    {
        //
        // 摘要:
        //     保存委托
        public delegate void IsSave();

        //
        // 摘要:
        //     配置文件路径
        public string Path { get; set; }

        public T Setting { get; set; }

        public T Value => Setting;

        //
        // 摘要:
        //     保存事件
        public event IsSave IsSaveEvent;

        public ConfigurationJson(string path)
        {
            Path = path;
        }

        public T Build()
        {
            StreamReader streamReader = new StreamReader(Path, Encoding.UTF8);
            string value = streamReader.ReadToEnd();
            streamReader.Close();
            Setting = JsonConvert.DeserializeObject<T>(value);
            return Setting;
        }

        public void Save()
        {
            string contents = JsonConvert.SerializeObject(Setting);
            if (!File.Exists(Path))
            {
                new FileStream(Path, FileMode.Create, FileAccess.ReadWrite).Close();
            }

            File.WriteAllText(Path, contents);
            this.IsSaveEvent?.Invoke();
        }
    }
}
