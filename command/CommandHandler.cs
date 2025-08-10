using System.Text;
using GigaChatAdapter.Auth;
using GigaChatAdapter;
using Spire.Doc.Documents;
using Spire.Doc;
using System.Text.RegularExpressions;
using System.Security.Cryptography.X509Certificates;
using ThemeBuilder.model;

namespace ThemeBuilder.command
{
    internal static class CommandHandler
    {
        static string? gigaKey = Environment.GetEnvironmentVariable("KEY_BUILDER", EnvironmentVariableTarget.User);
        static Authorization authorization = new(gigaKey!, RateScope.GIGACHAT_API_PERS);

        static readonly TemplateModelHandler<GigaChatModel> gigaModel = new();
        static readonly TemplateModelHandler<DocumentModel> documentModel = new();

        static readonly string[] separator = ["\r\n", "\n"];
        static string theme = "Не установлена";
        static string[] list = [];
        static int take = 0;

        public static async Task DownloadCertificate()
        {
            string link = "https://gu-st.ru/content/lending/russian_trusted_root_ca_pem.crt";

            using var httpClient = new HttpClient();
            byte[] certificateBytes = await httpClient.GetByteArrayAsync(link);

            var certificate = new X509Certificate2(certificateBytes);

            using var store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);
            store.Add(certificate);
            store.Close();
        }

        public static void GetHelpList()
        {
            ColorizeText.MessagePrint("   --help | получить список команд\n" +
                                      "   --detail | получить список переменных \n" +
                                      "   --key set <value> | установить API ключ к GigaChat\n" +
                                      "   --theme accessor <value> | установить/получить искомую тему\n" +
                                      "   --list | получить декомпозицию темы\n" +
                                      "   --take accessor <value>  | установить/получить значение переменной охвата\n" +
                                      "   --build | выполнить поиск информации и сформировать отчет\n" +
                                      "   --default | вернуться к заводским настройкам\n\n" +
                                      "   --model accessor <value> | установить/получить AI модель\n" +
                                      "   --temperature accessor <value> | установить/получить температуру ответа\n" +
                                      "   --topP accessor <value> | установить/получить topP параметр\n" +
                                      "   --maxTokens accessor <value> | установить/получить количество токенов\n\n" +
                                      "   --doc.format accessor <value> | установить/получить формат документа\n" +
                                      "   --doc.margin accessor <left; top; right; bottom> | установить/получить отступы от краев листа\n" +
                                      "   --doc.aligment accessor <value> | установить/получить формат выравнивания\n" +
                                      "   --doc.firstLine accessor <value> | установить/получить отступ первой строки\n" +
                                      "   --doc.spacing accessor <value> | установить/получить межстрочный интервал\n" +
                                      "   --doc.family accessor <value> | установить/получить шрифт документа\n" +
                                      "   --doc.size accessor <value> | установить/получить размер шрифта\n", ColorizeText.colors["System"]);
        }

        public static void GetDetailList()
        {
            ColorizeText.MessagePrint($"   Тема: {theme}\n" +
                                      $"   Размерность списка: {list.Length}\n" +
                                      $"   Take параметр: {take}\n\n", ColorizeText.colors["System"]);

            ColorizeText.MessagePrint($"{gigaModel.GetModel()}\n" +
                                      $"{documentModel.GetModel()}\n", ColorizeText.colors["System"]);
        }

        public static async Task SendRequest() => await authorization.SendRequest();

        public static async Task SetKey(string key)
        {
            Authorization auth = new(key, RateScope.GIGACHAT_API_PERS);

            await auth.SendRequest();
            var token = auth.LastResponse.GigaChatAuthorizationResponse?.AccessToken;
            if (string.IsNullOrEmpty(token))
            {
                ColorizeText.MessagePrint("[ERROR] Токен доступа отсутствует или не был получен\n", ColorizeText.colors["Warning"]);
                return;
            }

            authorization = auth;
            gigaKey = key;
            Environment.SetEnvironmentVariable("KEY_BUILDER", key, EnvironmentVariableTarget.User);
            ColorizeText.MessagePrint("[OK] Подключение установлено\n", ColorizeText.colors["Success"]);
        }

        public static void GetTheme()
        {
            if (string.IsNullOrEmpty(theme))
            {
                ColorizeText.MessagePrint("[ERROR] Тема отсутствует\n", ColorizeText.colors["Warning"]);
                return;
            }

            ColorizeText.MessagePrint($"   Искомая тема: {theme}\n", ColorizeText.colors["System"]);
        }

        public static async Task SetTheme(string value)
        {
            if (string.IsNullOrEmpty(gigaKey))
            {
                ColorizeText.MessagePrint("[ERROR] Ключ API отсутствует\n", ColorizeText.colors["Warning"]);
                return;
            }

            theme = value;
            string? decomposition = await ResponseGigaChat($"Тема исследования: {theme}\n{PromptManager.ThemeDecomposition}");
            list = ToList(decomposition!);

            ColorizeText.MessagePrint("[OK] Тема добавлена, а список сформирован\n", ColorizeText.colors["Success"]);
        }

        public static void GetList()
        {
            if (list.Length == 0)
            {
                ColorizeText.MessagePrint("[ERROR] Список пуст\n", ColorizeText.colors["Warning"]);
                return;
            }

            foreach (var s in list)
                ColorizeText.MessagePrint($"   {s}\n", ColorizeText.colors["System"]);
        }

        public static void GetTake() => ColorizeText.MessagePrint($"   Значение переменной охвата: {take}\n", ColorizeText.colors["System"]);

        public static void SetTake(string value)
        {
            if (!int.TryParse(value, out int num) || num < 0)
            {
                ColorizeText.MessagePrint("[ERROR] Параметр охвата некорректен\n", ColorizeText.colors["Warning"]);
                return;
            }

            ColorizeText.MessagePrint("[OK] Параметр охвата был изменен\n", ColorizeText.colors["Success"]);
            take = num;
        }

        public static async Task Build()
        {
            if (list.Length == 0)
            {
                ColorizeText.MessagePrint("[ERROR] Список пуст. Анализ темы невозможен\n", ColorizeText.colors["Warning"]);
                return;
            }

            string content = await GetSearchContent();
            Builder(content);
        }

        public static void Default()
        {
            theme = "Hello world";
            list = [];
            take = 0;

            gigaModel.Default();
            documentModel.Default();

            ColorizeText.MessagePrint($"[OK] Все параметры были изменены на заводские значения\n", ColorizeText.colors["Success"]);
        }

        private static void GetPropetry(object model, string propetry)
        {
            ColorizeText.MessagePrint($"   Значение {propetry}: {model.GetType().GetMethod("Get")?.Invoke(model, [propetry])}\n",
                ColorizeText.colors["System"]);
        }

        private static void SetPropetry(object model, string property, object value)
        {
            model.GetType().GetMethod("Set")?.Invoke(model, [property, value]);
            ColorizeText.MessagePrint($"[OK] Параметр {property} был изменён\n", ColorizeText.colors["Success"]);
        }

        public static void GetModel() => GetPropetry(gigaModel, "Model");

        public static async Task SetModel(string value)
        {
            if (await IsModel(value))
                SetPropetry(gigaModel, "Model", value);
        }

        private static async Task<bool> IsModel(string model)
        {
            var current = gigaModel.Get("Model");

            gigaModel.Set("Model", model);

            var response = await ResponseGigaChat(null!);
            if (string.IsNullOrEmpty(response))
            {
                ColorizeText.MessagePrint("[ERROR] Данная модель отсутствует\n", ColorizeText.colors["Warning"]);
                gigaModel.Set("Model", current);
                return false;
            }

            return true;
        }

        public static void GetTemperature() => GetPropetry(gigaModel, "Temperature");

        public static void SetTemperature(string value)
        {
            string propetry = "Temperature";
            if (IsValid(propetry, value, 0, 2, typeof(float)))
                SetPropetry(gigaModel,propetry, value);
        }

        public static void GetTopP() => GetPropetry(gigaModel, "TopP");

        public static void SetTopP(string value)
        {
            string propetry = "TopP";
            if (IsValid(propetry, value, 0, 1, typeof(float)))
                SetPropetry(gigaModel, propetry, value);
        }

        public static void GetMaxTokens() => GetPropetry(gigaModel, "MaxTokens");

        public static void SetMaxTokens(string value)
        {
            string propetry = "MaxTokens";
            if (IsValid(propetry, value, 0, 10000, typeof(long)))
                SetPropetry(gigaModel, propetry, value);
        }

        private static bool IsValid(string param, string value, double min, double max, Type type)
        {
            bool isValid = false;

            if (type == typeof(float) && float.TryParse(value, out float F))
            {
                isValid = F >= min && F <= max;
            }
            else if (type == typeof(long) && long.TryParse(value, out long L))
            {
                isValid = L >= min && L <= max;
            }

            if (!isValid)
                ColorizeText.MessagePrint($"[ERROR] Значение {param} должно быть от {min} до {max}\n", ColorizeText.colors["Warning"]);

            return isValid;
        }

        private static void NotSupportFormat() => ColorizeText.MessagePrint($"[ERROR] Этот параметр не поддерживается\n", ColorizeText.colors["Warning"]);

        public static void GetFormat() => GetPropetry(documentModel, "Format");

        public static void SetFormat(string value)
        {
            var format = ConvertToFileFormat(value);
            if (format != null)
            {
                SetPropetry(documentModel, "Format", format);
            }
            else
            {
                NotSupportFormat();
            }
        }

        private static FileFormat? ConvertToFileFormat(string value)
        {
            return value switch
            {
                "doc" => FileFormat.Doc,
                "docx" => FileFormat.Docx,
                "docx2010" => FileFormat.Docx2010,
                "docx2013" => FileFormat.Docx2013,
                "docx2016" => FileFormat.Docx2016,
                "docx2019" => FileFormat.Docx2019,
                _ => null
            };
        }

        public static void GetMargin() => GetPropetry(documentModel, "Margin");

        public static void SetMargin(string value)
        {
            var margins = ConvertToMargins(value);
            if (margins != null)
                SetPropetry(documentModel, "Margin", margins);
        }

        private static DocumentMargins? ConvertToMargins(string value)
        {
            var parts = value.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (parts.Length != 4)
            {
                ColorizeText.MessagePrint("[ERROR] Значение Margin должно иметь формат <left; top; right; bottom>\n", ColorizeText.colors["Warning"]);
                return null;
            }

            if (!float.TryParse(parts[0], out float left) ||
                !float.TryParse(parts[1], out float top) ||
                !float.TryParse(parts[2], out float right) ||
                !float.TryParse(parts[3], out float bottom))
            {
                ColorizeText.MessagePrint("[ERROR] Не все значения отступа можно преобразовать в число\n", ColorizeText.colors["Warning"]);
                return null;
            }

            float[] margins = { left, top, right, bottom };
            if (margins.Any(m => m < 0 || m > 5))
            {
                ColorizeText.MessagePrint("[ERROR] Все значения отступов должны быть в диапазоне от 0 до 5\n", ColorizeText.colors["Warning"]);
                return null;
            }

            return new DocumentMargins
            {
                Left = left,
                Top = top,
                Right = right,
                Bottom = bottom
            };
        }

        public static void GetAligment() => GetPropetry(documentModel, "Aligment");

        public static void SetAligment(string value)
        {
            var alignment = ConvertToAlignment(value);
            if (alignment != null)
            {
                SetPropetry(documentModel, "Aligment", alignment);
            }
            else
            {
                NotSupportFormat();
            }
        }

        private static HorizontalAlignment? ConvertToAlignment(string value)
        {
            return value switch
            {
                "left" => HorizontalAlignment.Left,
                "right" => HorizontalAlignment.Right,
                "center" => HorizontalAlignment.Center,
                "justify" => HorizontalAlignment.Justify,
                _ => null
            };
        }

        public static void GetFirstLine() => GetPropetry(documentModel, "FirstLine");

        public static void SetFirstLine(string value)
        {
            string propetry = "FirstLine";
            if (IsValid(propetry, value, 0, 10, typeof(float)))
                SetPropetry(documentModel, propetry, value);
        }

        public static void GetSpacing() => GetPropetry(documentModel, "LineSpacing");

        public static void SetSpacing(string value)
        {
            string propetry = "LineSpacing";
            if (IsValid(propetry, value, 1, 5, typeof(float)))
                SetPropetry(documentModel, "LineSpacing", value);
        }

        public static void GetFamily() => GetPropetry(documentModel, "FontFamily");

        public static void SetFamily(string value) => SetPropetry(documentModel, "FontFamily", value);

        public static void GetSize() => GetPropetry(documentModel, "FontSize");

        public static void SetSize(string value)
        {
            string propetry = "FontSize";
            if (IsValid(propetry, value, 1, 150, typeof(float)))
                SetPropetry(documentModel, "FontSize", value);
        }

        private static async Task<string?> ResponseGigaChat(string prompt)
        {
            Completion completion = new();
            CompletionSettings settings = new((string)gigaModel.Get("Model"),
                                              (float)gigaModel.Get("Temperature"),
                                              (float)gigaModel.Get("TopP")!,
                                              1,
                                              (long)gigaModel.Get("MaxTokens"));

            var result = await completion.SendRequest(
            authorization.LastResponse.GigaChatAuthorizationResponse?.AccessToken!,
            prompt, true, settings
            );

            return result.GigaChatCompletionResponse?.Choices.LastOrDefault()?.Message.Content;
        }

        private static async Task<string> GetSearchContent()
        {
            StringBuilder temp = new();

            foreach (string L in take == 0 ? list : list.Take(take))
            {
                if (!L.StartsWith('*'))
                    temp.Append(L + '\n');

                temp.Append(await ResponseGigaChat($"Тема: {L}\n{PromptManager.ThemeSearch}") + '\n');
                ColorizeText.MessagePrint($"[OK] Тема \"{L}\" была исследована\n", ColorizeText.colors["Success"]);
            }

            string content = RemoveSpaceParagraphs(temp.ToString());
            content = RemoveMarkdown(content);
            return content;
        }

        private static string[] ToList(string list)
        {
            return list.Split(separator, StringSplitOptions.RemoveEmptyEntries)
                       .Select(line => line.Trim())
                       .Where(line => !string.IsNullOrWhiteSpace(line))
                       .ToArray();

        }

        private static string RemoveSpaceParagraphs(string content)
        {
            return string.Join("\n", content.Split(separator, StringSplitOptions.RemoveEmptyEntries)
                                            .Select(line => line.Trim()));
        }

        private static string RemoveMarkdown(string content)
        {
            var lines = content.Split(separator, StringSplitOptions.RemoveEmptyEntries)
                               .Where(line => !Regex.IsMatch(line.TrimStart(), @"^#{1,}\s?") &&
                                              !Regex.IsMatch(line.TrimStart(), @"^\*{1,}\s?"))
                               .Select(line => line.Trim());

            string removeSym = string.Join("\n", lines);

            removeSym = Regex.Replace(removeSym, @"#{1,}", "");
            removeSym = Regex.Replace(removeSym, @"\*{1,}", "");
            removeSym = removeSym.Replace("<", "«")
                                 .Replace(">", "»");

            return removeSym;
        }

        private static void Builder(string content)
        {
            Document doc = DocumentModel.Builder(documentModel.GetModel(), content);

            string desktopFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string file = Path.Combine(desktopFolder, $"{theme}.docx");

            doc.SaveToFile(file, (FileFormat)documentModel.Get("Format"));
            doc.Close();

            ColorizeText.MessagePrint($"[OK] Отчет был сохранен на рабочий стол! [{DateTime.Now}]\n", ColorizeText.colors["Success"]);
        }

    }
}