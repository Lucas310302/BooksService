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
using System.Runtime.CompilerServices;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Win32;
using System.IO;

namespace BookServiceClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly HttpClient _httpClient;
        private string _token;

        public MainWindow()
        {
            InitializeComponent();

            LoginGrid.Visibility = Visibility.Visible;
            SignupGrid.Visibility = Visibility.Collapsed;
            HeaderGrid.Visibility = Visibility.Collapsed;
            BooksGrid.Visibility = Visibility.Collapsed;

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            _httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://localhost:7275") };
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
                // Create horizontal stackpanel for pdf button
                DockPanel outerStackPanel = new DockPanel
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };

                // Create a StackPanel for title and author
                StackPanel bookStackPanel = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    VerticalAlignment = VerticalAlignment.Center
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
                    Width = 35,
                    Height = 35,
                    Cursor = Cursors.Hand,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    ToolTip = "Download PDF"
                };

                // Setup clickevent
                pdfBtn.MouseLeftButtonDown += (obj, args) => PdfView_Click(obj, args, book.Id);

                outerStackPanel.Children.Add(bookStackPanel);
                outerStackPanel.Children.Add(pdfBtn);

                // Create border
                Border bookBorder = new Border
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(1),
                    Margin = new Thickness(5),
                    Padding = new Thickness(10),
                    Background = Brushes.White,
                    Width = itemWidth
                };

                // Add Stackpanel with Title and Author to Border
                bookBorder.Child = outerStackPanel;

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

        private string ParseJwtToken()
        {
            var handler = new JwtSecurityTokenHandler();
            if (handler.CanReadToken(_token))
            {
                var jwtToken = handler.ReadJwtToken(_token); // Decoder for token

                // Extract role
                var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "role" || c.Type == ClaimTypes.Role);
                var role = roleClaim?.Value;

                if (role != null)
                {
                    return role;
                }
                else
                {
                    MessageBox.Show("Role not found in the token");
                    return null;
                }
            }
            else
            {
                MessageBox.Show("Invalid token format");
                return null;
            }
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var username = LoginUsernameInput.Text;
            var password = LoginPasswordInput.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please fill out both username and password", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    _token = await response.Content.ReadAsStringAsync();

                    if (!string.IsNullOrEmpty(_token))
                    {
                        // Store token in memory
                        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

                        // Loadbooks when token is fetched
                        LoadBooks();

                        // Hide login and show books
                        LoginGrid.Visibility = Visibility.Collapsed;
                        HeaderGrid.Visibility = Visibility.Visible;
                        BooksGrid.Visibility = Visibility.Visible;

                        // Check if admin if user should have admin controls
                        if (ParseJwtToken() == "Admin")
                            AdminBtn.Visibility = Visibility.Visible;
                        else
                        {
                            AdminBtn.Visibility = Visibility.Collapsed;
                            ViewAllBooks.Margin = new Thickness(0, 0, 580, 0);
                            ViewAllBooks.Width = 200;
                        }
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

        // Change page to sign up
        private void GotoSignupPage_Click(object sender, RoutedEventArgs args)
        {
            LoginGrid.Visibility = Visibility.Collapsed;
            SignupGrid.Visibility = Visibility.Visible;
        }

        private async void SignUpButton_Click(object sender, RoutedEventArgs args)
        {
            var username = SignUpUsernameInput.Text;
            var password = SignUpPasswordInput.Text;
            var confirmpassword = SignUpConfirmPasswordInput.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmpassword))
            {
                MessageBox.Show("Please fill out all fields", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (confirmpassword != password)
            {
                MessageBox.Show("Passwords do not match", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Create signup dto
            var signupDto = new SignupDto
            {
                Username = username,
                Password = password,
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/users/signup", signupDto);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("User Created :)", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    SignupGrid.Visibility = Visibility.Collapsed;
                    LoginGrid.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Change page to login
        private void GotoLoginPage_Click(object sender, RoutedEventArgs args)
        {
            SignupGrid.Visibility = Visibility.Collapsed;
            LoginGrid.Visibility = Visibility.Visible;
        }

        private void OpenAccPanel_Click(object sender, RoutedEventArgs e)
        {
            var role = ParseJwtToken();

            if (role == "Admin")
            {
                var adminpanel = new AdminPanel(_token, _httpClient);
                adminpanel.Show();
            }
        }

        private void ViewAllBooks_Click(object sender, RoutedEventArgs e)
        {
            LoadBooks();
        }

        private async void PdfView_Click(object sender, RoutedEventArgs e, int Id)
        {
            if (ParseJwtToken() == "Admin")
            {
                MessageBox.Show($"ID: {Id.ToString()}");

                var result = MessageBox.Show("Do you want to download the PDF?", "Download Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                {
                    return;
                }
            }

            try
            {
                var response = await _httpClient.GetAsync($"api/Books/download/{Id}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsByteArrayAsync();

                    var saveFileDialog = new SaveFileDialog
                    {
                        FileName = $"{Id}.pdf",
                        DefaultExt = ".pdf",
                        Filter = "PDF documents (.pdf)|*.pdf"
                    };

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        File.WriteAllBytes(saveFileDialog.FileName, content);
                        MessageBox.Show("PDF download successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show($"Failed to download PDF: {response.ReasonPhrase}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occured: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
