using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Http.Json;

namespace BookServiceClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly HttpClient _httpClient;

        public MainWindow()
        {
            InitializeComponent();

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            _httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://localhost:7275") };

            // Load Books on startup
            LoadBooks();
        }

        private async void LoadBooks()
        {
            try
            {
                var books = await _httpClient.GetFromJsonAsync<List<Book>>("api/Books");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Can't fetch books; " + ex);
                Clipboard.SetText(ex.ToString());
                Console.WriteLine(ex);
            }
        }

        private async void SearchBooks_Click(object sender, RoutedEventArgs e)
        {
            var searchInput = SearchInput.Text;

            if (string.IsNullOrWhiteSpace(searchInput))
            {
                MessageBox.Show("Please fill out the search field");
                return;
            }
        }

        public class Book
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Author { get; set; }
            public bool isAvailable { get; set; }
        }
    }
}
