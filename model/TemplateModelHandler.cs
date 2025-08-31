using System.Text.Encodings.Web;
using System.Text.Json;

namespace ThemeBuilder.model
{
    internal class TemplateModelHandler<T>
    {
        private readonly string path = Path.Combine($@"..\..\..\cfg\{typeof(T).Name}.json");
        private T? model;

        public TemplateModelHandler() { model = Read(); }

        public T Read()
        {
            if (!File.Exists(path))
            {
                string? directory = Path.GetDirectoryName(path);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory!);
                }
                Default();
            }

            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<T>(json)!;
        }

        public void Set(string param, object value)
        {
            var property = typeof(T).GetProperty(param);
            var convertedValue = Convert.ChangeType(value, property!.PropertyType);
            property.SetValue(model, convertedValue);

            Save();
        }

        public object Get(string param)
        {
            var property = typeof(T).GetProperty(param);
            return property!.GetValue(model)!;
        }

        public T GetModel() => model!;

        public void Default()
        {
            Type type = typeof(T);
            model = (T)type.GetMethod("SetDefault")!.Invoke(null, null)!;

            Save();
        }

        public void Save()
        {
            string json = JsonSerializer.Serialize(model, JsonOptions);
            File.WriteAllText(path, json);
        }

        private readonly JsonSerializerOptions JsonOptions = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true
        };
    }

}
