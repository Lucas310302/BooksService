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

            // Hide bookpage from the beginning
            SearchPage.Visibility = Visibility.Collapsed;
            AllBooksPage.Visibility = Visibility.Visible;

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

                // Put books into the all books panel

                // Clear children just in case
                AllBooksPanel.Children.Clear();

                double wrapPanelWidth = AllBooksPanel.Width;
                double itemWidth = wrapPanelWidth / 3 - 20;

                // Generate book element for each book
                foreach (var book in books)
                {
                    // Create border
                    Border bookBorder = new Border
                    {
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(1),
                        Margin = new Thickness(5),
                        Padding = new Thickness(10),
                        Background = Brushes.LightGray,
                        Width = itemWidth
                    };

                    // Create a StackPanel for title and author
                    StackPanel bookStackPanel = new StackPanel
                    {
                        Orientation = Orientation.Vertical
                    };

                    // Title
                    TextBlock titleText = new TextBlock
                    {
                        Text = book.Title,
                        FontWeight = FontWeights.Bold,
                        Margin = new Thickness(0, 0, 0, 5)
                    };
                    bookStackPanel.Children.Add(titleText);

                    // Author
                    TextBlock authorText = new TextBlock
                    {
                        Text = book.Author,
                        Margin = new Thickness(0, 0, 0, 5)
                    };
                    bookStackPanel.Children.Add(authorText);

                    // Add Stackpanel with Title and Author to Border
                    bookBorder.Child = bookStackPanel;

                    // Add the border to the BooksPanel
                    AllBooksPanel.Children.Add(bookBorder);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Can't fetch books; " + ex);
                Clipboard.SetText(ex.ToString());
                Console.WriteLine(ex);
            }
        }

        private async void SearchBooks_KeyDown(object sender, KeyEventArgs e)
        {
            // Return if enter key hasn't been pressed
            if (e.Key != Key.Enter)
            {
                return;
            }

            var searchInput = SearchInput.Text;

            if (string.IsNullOrWhiteSpace(searchInput))
            {
                MessageBox.Show("Please fill out the search field");
                return;
            }

            try
            {
                // Hide FrontPage and show BookPage
                AllBooksPage.Visibility = Visibility.Collapsed;
                SearchPage.Visibility = Visibility.Visible;

                //Api call with query string parameter
                var response = await _httpClient.GetAsync($"api/Books/search?query={Uri.EscapeDataString(searchInput)}");

                // Get books from response body
                var books = await response.Content.ReadFromJsonAsync<List<Book>>();

                // Clear prev book display
                FetchedBooksDisplayPanel.Children.Clear();

                // Set the query display to the query
                SearchDisplay.Text = $"\"{searchInput}\"";

                // Get the width of the WrapPanel and make sure exactly 3 books will fit inside
                double wrapPanelWidth = FetchedBooksDisplayPanel.Width;
                double itemWidth = wrapPanelWidth / 3 - 20; // Subtract margin and padding and divide by 3

                // Generate book element for each book
                foreach (var book in books)
                {
                    // Create border
                    Border bookBorder = new Border
                    {
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(1),
                        Margin = new Thickness(5),
                        Padding = new Thickness(10),
                        Background = Brushes.LightGray,
                        Width = itemWidth
                    };

                    // Create a StackPanel for title and author
                    StackPanel bookStackPanel = new StackPanel
                    {
                        Orientation = Orientation.Vertical
                    };

                    // Title
                    TextBlock titleText = new TextBlock
                    {
                        Text = book.Title,
                        FontWeight = FontWeights.Bold,
                        Margin = new Thickness(0, 0, 0, 5)
                    };
                    bookStackPanel.Children.Add(titleText);

                    // Author
                    TextBlock authorText = new TextBlock
                    {
                        Text = book.Author,
                        Margin = new Thickness(0, 0, 0, 5)
                    };
                    bookStackPanel.Children.Add(authorText);

                    // Add Stackpanel with Title and Author to Border
                    bookBorder.Child = bookStackPanel;

                    // Add the border to the BooksPanel
                    FetchedBooksDisplayPanel.Children.Add(bookBorder);
                }
            }
            catch (HttpRequestException reqEx)
            {
                MessageBox.Show($"Error fetching books: {reqEx.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception: {ex.Message}");
            }
        }

        public class Book
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Author { get; set; }
            public bool isAvailable { get; set; }
        }

        private void ViewAllBooks_Click(object sender, RoutedEventArgs e)
        {
            SearchPage.Visibility = Visibility.Collapsed;
            AllBooksPage.Visibility = Visibility.Visible;
        }
    }
}
