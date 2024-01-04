namespace AzureAISearchExample
{
    public class FilePickerService
    {
        public Dictionary<string, byte[]> ShowOpenFileDialog()
        {
            using var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "PDF and Text files (*.pdf;*.txt)|*.pdf;*.txt|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Multiselect = true;
            var result = new Dictionary<string, byte[]>();
            if (openFileDialog.ShowDialog() != DialogResult.OK) return [];
            var fileNames = openFileDialog.FileNames;
            foreach (var fileName in fileNames)
            {
                var fileBytes = File.ReadAllBytes(fileName);
                result[fileName] = fileBytes;
            }
            return result;

        }
    }
}
