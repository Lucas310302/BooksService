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
using System.Net.Http.Headers;
using BookServiceClient.Dtos;

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

            LoginGrid.Visibility = Visibility.Collapsed;

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
                // Load Books
                var books = await _httpClient.GetFromJsonAsync<List<Book>>("api/Books");

                // Put books in book panel list
                SetFetchedBooksInList(books, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Can't fetch books; " + ex.Message);
                Clipboard.SetText(ex.Message);
            }
        }

        // Put books dynamically in book panel list
        private void SetFetchedBooksInList(List<Book> books, string searchInput)
        {
            // Set Query display string, if there isn't one then display "all books"
            if (!string.IsNullOrEmpty(searchInput))
                SearchDisplay.Text = $"\"{searchInput}\"";
            else
                SearchDisplay.Text = "All Books";

            // Clear prev book display
            BooksPanel.Children.Clear();

            // Get the width of the WrapPanel and make sure exactly 3 books will fit inside
            double wrapPanelWidth = BooksPanel.Width;
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

                // PDF Icon button
                Image pdfBtn = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/icons/pdf.png")),
                    Width = 100,
                    Height = 100
                };

                // Setup clickevent
                pdfBtn.MouseLeftButtonDown += (obj, args) => PdfView_Click(obj, args, book.Id);

                bookStackPanel.Children.Add(pdfBtn);

                // Add Stackpanel with Title and Author to Border
                bookBorder.Child = bookStackPanel;

                // Add the border to the BooksPanel
                BooksPanel.Children.Add(bookBorder);
            }
        }

        private async void SearchBooks_KeyDown(object sender, KeyEventArgs e)
        {
            // Return if enter key hasn't been pressed
            if (e.Key != Key.Enter)
            {
                return;
            }

            // Get Search Query
            var searchInput = SearchInput.Text;

            // Check if search query is empty
            if (string.IsNullOrWhiteSpace(searchInput))
            {
                MessageBox.Show("Please fill out the search field");
                return;
            }

            try
            {
                //Api call with query string parameter
                var response = await _httpClient.GetAsync($"api/Books/search?query={Uri.EscapeDataString(searchInput)}");

                // Get books from response body
                var books = await response.Content.ReadFromJsonAsync<List<Book>>();

                // Put books dynamically in a list
                SetFetchedBooksInList(books, searchInput);
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

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var username = UsernameInput.Text;
            var password = PasswordInput.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please fill out both username and password");
                return;
            }

            // Create login dto
            var loginDto = new LoginDto
            {
                Username = username,
                Password = password
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/users/login", loginDto);

                if (response.IsSuccessStatusCode)
                {
                    var token = await response.Content.ReadAsStringAsync();

                    if (!string.IsNullOrEmpty(token))
                    {
                        // Store token in memory
                        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                        // Hide login and show books
                        LoginGrid.Visibility = Visibility.Collapsed;
                        HeaderGrid.Visibility = Visibility.Visible;
                        AllBooksPage.Visibility = Visibility.Visible;

                        MessageBox.Show("Login successful");
                    }
                    else
                    {
                        MessageBox.Show("Login failed; No token recieved");
                    }
                }
                else
                {
                    MessageBox.Show("Login failed; Invalid username or password");
                }
            }
            catch (HttpRequestException reqEx)
            {
                MessageBox.Show($"Error connecting to server: {reqEx.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An Error Occured: {ex.Message}");
            }
        }

        private void ViewAllBooks_Click(object sender, RoutedEventArgs e)
        {
            LoadBooks();
        }

        private void PdfView_Click(object sender, RoutedEventArgs e, int Id)
        {
            MessageBox.Show(Id.ToString());
        }

        private void AccountBtn_Click(object sender, RoutedEventArgs e)
        {
            // Hide everything except the login screen
            HeaderGrid.Visibility = Visibility.Collapsed;
            AllBooksPage.Visibility = Visibility.Collapsed;
            LoginGrid.Visibility = Visibility.Visible;
        }

        private void AccBackButton_Click(object sender, RoutedEventArgs e)
        {
            // Go back from the login screen
            LoginGrid.Visibility = Visibility.Collapsed;
            AllBooksPage.Visibility = Visibility.Visible;
            HeaderGrid.Visibility = Visibility.Visible;
        }

        public class Book
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Author { get; set; }
            public bool isAvailable { get; set; }
            public byte[] PDFfile { get; set; }
        }
    }
}
