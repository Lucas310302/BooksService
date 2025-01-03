using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;
using BookServiceClient.Dtos;
using Microsoft.AspNetCore.Http.Internal;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;

namespace BookServiceClient
{
    /// <summary>
    /// Interaction logic for AdminPanel.xaml
    /// </summary>
    public partial class AdminPanel : Window
    {
        private readonly HttpClient _httpClient;
        private string _token;

        public AdminPanel(string token, HttpClient httpClient)
        {
            InitializeComponent();

            // Get the admin token so the adminpanel can still run admin priv commands, and get the connection
            _token = token;
            _httpClient = httpClient;

            AddMenu.Visibility = Visibility.Collapsed;
            RemoveMenu.Visibility = Visibility.Collapsed;
            ModifyMenu.Visibility = Visibility.Collapsed;
            StartMenu.Visibility = Visibility.Visible;
        }

        private void GotoAddBookPage_Click(object sender, RoutedEventArgs args)
        {
            StartMenu.Visibility = Visibility.Collapsed;
            AddMenu.Visibility = Visibility.Visible;
        }

        private string _selectedFilePath;

        // Uploading pdf logic via openfiledialog
        private void UploadPDF_Click(object sender, RoutedEventArgs args)
        {
            // Only let the user pick pdf
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                Title = "Select a PDF file"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _selectedFilePath = openFileDialog.FileName;
                PDFUpload.Content = $"Selected: {openFileDialog.FileName}";
            }
        }

        // Checks if form is valid before sending to server
        private async void SubmitBook_Click(object sender, RoutedEventArgs args)
        {
            // Sets up form variables
            string title = TitleInput.Text;
            string author = AuthorInput.Text;
            bool isAvailable = IsAvailabelCheckBox.IsChecked ?? false;

            // Check if filepath
            if (string.IsNullOrWhiteSpace(_selectedFilePath))
            {
                MessageBox.Show("Please select a PDF file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Check if title and author
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(author))
            {
                MessageBox.Show("Please fill out both title and author.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Try upload book if everything is there
            try
            {
                await UploadBookAsync(_selectedFilePath, title, author, isAvailable);
                MessageBox.Show("Book Uploaded successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Upload Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Sends book to server
        private async Task UploadBookAsync(string filePath, string title, string author, bool isAvailable)
        {
            using (var form = new MultipartFormDataContent())
            {
                // Add pdf file
                var fileContent = new ByteArrayContent(await Task.Run(() => File.ReadAllBytes(_selectedFilePath)));
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
                form.Add(fileContent, "pdfFile", System.IO.Path.GetFileName(filePath));

                // Add other fields
                form.Add(new StringContent(title), "Title");
                form.Add(new StringContent(author), "Author");
                form.Add(new StringContent(isAvailable.ToString()), "IsAvailable");

                // Post to server
                var response = await _httpClient.PostAsync("api/Books/add", form);
                response.EnsureSuccessStatusCode();
            }
        }

        // Goto remove page on admin panel
        private void GotoRemovePage_Click(object sender, RoutedEventArgs e)
        {
            StartMenu.Visibility = Visibility.Collapsed;
            RemoveMenu.Visibility = Visibility.Visible;
        }

        // Check if form is valid before sending to server
        private async void SubmitDelete_Click(object sender, RoutedEventArgs args)
        {
            string input = IDsRemoveInput.Text;

            if (string.IsNullOrWhiteSpace(input))
            {
                MessageBox.Show("Please enter valid IDs.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var ids = ParseIds(input);

                if (ids == null || !ids.Any())
                {
                    MessageBox.Show("No Valid Ids found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                await DeleteBooksAsync(ids);
                MessageBox.Show($"Deleted {ids.Count} files successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Delete Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Parses ids so the server can read the format
        private List<int> ParseIds(string input)
        {
            var ids = new List<int>();

            // Split by comma
            var parts = input.Split(',');

            foreach (var part in parts)
            {
                if (part.Contains("-"))
                {
                    // Handle Range
                    var range = part.Split('-');
                    if (range.Length == 2 && int.TryParse(range[0], out int start) && int.TryParse(range[1], out int end))
                    {
                        ids.AddRange(Enumerable.Range(start, end - start + 1));
                    }
                }
                else if (int.TryParse(part, out int id))
                {
                    ids.Add(id);
                }
            }

            return ids.Distinct().ToList(); // Remove duplicates
        }

        // Sends the parsed ids to server and lets the server delete the books
        private async Task DeleteBooksAsync(List<int> ids)
        {
            var query = string.Join("&", ids.Select(id => $"ids={id}")); // Create query
            var response = await _httpClient.DeleteAsync($"api/Books/delete?{query}");
            response.EnsureSuccessStatusCode();
        }

        // Goto modify page
        private void GotoModifyPage_Click(Object sender, RoutedEventArgs args)
        {
            // Collapse startmenu and make modify page visible
            StartMenu.Visibility = Visibility.Collapsed;
            ModifyMenu.Visibility = Visibility.Visible;
        }

        // Get pdf file and validate
        private FileInfo _selectedPdfFile;
        private void ModifyPDF_Click(object sender, RoutedEventArgs args)
        {
            var openfiledialog = new OpenFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                Title = "Select a PDF File"
            };

            if (openfiledialog.ShowDialog() == true)
            {
                _selectedPdfFile = new FileInfo(openfiledialog.FileName);
                ModifyPDFUpload.Content = $"Selected: {openfiledialog.FileName}";
                MessageBox.Show("PDF File Selected", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // Check values and then send to server
        private async void SubmitModifyBook_Click(object sender, RoutedEventArgs args)
        {
            try
            {
                var Id = int.Parse(BookIDInput.Text);
                var content = new MultipartFormDataContent();

                if (!string.IsNullOrWhiteSpace(ModifyTitleInput.Text))
                {
                    content.Add(new StringContent(ModifyTitleInput.Text), "Title");
                }

                if (!string.IsNullOrWhiteSpace(ModifyAuthorInput.Text))
                {
                    content.Add(new StringContent(ModifyAuthorInput.Text), "Author");
                }

                content.Add(new StringContent(ModifyIsAvailableCheckBox.IsChecked.HasValue.ToString()), "IsAvailable");

                if (_selectedPdfFile != null)
                {
                    var pdfContent = new StreamContent(_selectedPdfFile.OpenRead());
                    pdfContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
                    content.Add(pdfContent, "PDFfile", "file.pdf");
                }

                var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"api/Books/modify/{Id}")
                {
                    Content = content
                };

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var responsemessage = await response.Content.ReadAsStringAsync();
                    MessageBox.Show(responsemessage);
                    MessageBox.Show("Book updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Error updating book: {error}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Clipboard.SetText(error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Logging for multipartdatacontent
        private void LogMultipartFormDataContent(MultipartFormDataContent content)
        {
            // Create a list to store field names and values
            var formData = new List<string>();

            // Loop through each part of the MultipartFormDataContent
            foreach (var part in content)
            {
                // Check if the part is a StringContent (i.e., a text field)
                if (part is StringContent stringContent)
                {
                    // Read the value of the StringContent (the form field's value)
                    var value = stringContent.ReadAsStringAsync().Result;
                    formData.Add($"{part.Headers.ContentDisposition.Name}: {value}");
                }
                // Check if the part is a StreamContent (i.e., a file upload)
                else if (part is StreamContent streamContent)
                {
                    // You can log the file name, or you can read the file if needed
                    var fileName = part.Headers.ContentDisposition.FileName;
                    formData.Add($"{part.Headers.ContentDisposition.Name}: {fileName ?? "No file"}");
                }
            }

            // Now log the form data (e.g., print to console or show in message box)
            foreach (var field in formData)
            {
                Console.WriteLine(field);
            }

            // Optionally, you can display the data in a MessageBox
            string message = "Form Data:\n" + string.Join("\n", formData);
            MessageBox.Show(message, "Form Data", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
