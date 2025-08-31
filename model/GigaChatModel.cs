using ThemeBuilder.command;

namespace ThemeBuilder.model
{
    internal class GigaChatModel : IDefault<GigaChatModel>
    {
        public string? Model { get; set; }
        public float Temperature { get; set; }
        public float TopP { get; set; }
        public long MaxTokens { get; set; }
        public string? PromptDecomposition { get; set; }
        public string? PromptSearcher { get; set; }

        public static GigaChatModel SetDefault()
        {
            return new GigaChatModel
            {
                Model = "GigaChat:latest",
                Temperature = 0.7f,
                TopP = 0.9f,
                MaxTokens = 1024,
                PromptDecomposition = PromptManager.ThemeDecomposition,
                PromptSearcher = PromptManager.ThemeSearch
            };
        }

        public override string ToString()
        {
            return $"   Модель: {Model}\n" +
                   $"   Температура: {Temperature}\n" +
                   $"   TopP параметр: {TopP}\n" +
                   $"   Количество токенов: {MaxTokens}\n";
        }
    }
}
