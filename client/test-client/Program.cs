// client/test-client/Program.cs
/**
 * BTHL CheckGate - API Test Client Application
 * File: client/test-client/Program.cs
 * 
 * We are building a comprehensive test client that demonstrates all API capabilities
 * and provides a command-line interface for testing the BTHL CheckGate REST API.
 * Our client showcases proper authentication, error handling, and data presentation.
 * 
 * @author David St John <davestj@gmail.com>
 * @version 1.0.0
 * @since 2025-09-09
 * 
 * CHANGELOG:
 * 2025-09-09 - FEAT: Initial API test client with comprehensive endpoint coverage
 */

using System.CommandLine;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BTHLCheckGate.TestClient
{
    /// <summary>
    /// We implement our main program class that provides comprehensive API testing capabilities.
    /// Our design includes command-line argument parsing and interactive testing modes.
    /// </summary>
    public class Program
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private static readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// We define our main entry point with comprehensive command-line interface.
        /// Our implementation supports both interactive and automated testing scenarios.
        /// </summary>
        /// <param name="args">Command line arguments for test configuration</param>
        /// <returns>Exit code indicating test success or failure</returns>
        public static async Task<int> Main(string[] args)
        {
            // We create our command-line interface with comprehensive options
            var rootCommand = new RootCommand("BTHL CheckGate API Test Client")
            {
                new Option<string>("--base-url", () => "https://localhost:9300", "Base URL for the API"),
                new Option<string>("--token", "Bearer token for authentication"),
                new Option<string>("--username", "Username for authentication"),
                new Option<string>("--password", "Password for authentication"),
                new Option<bool>("--interactive", "Run in interactive mode"),
                new Option<string>("--output", "Output file for test results"),
                new Option<bool>("--verbose", "Enable verbose output")
            };

            // We configure our command handler to process all test scenarios
            rootCommand.SetHandler(async (string baseUrl, string token, string username, 
                string password, bool interactive, string outputFile, bool verbose) =>
            {
                var client = new ApiTestClient(baseUrl, verbose);
                
                try
                {
                    // We authenticate using provided credentials or token
                    if (!string.IsNullOrEmpty(token))
                    {
                        await client.SetBearerTokenAsync(token);
                    }
                    else if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                    {
                        await client.AuthenticateAsync(username, password);
                    }
                    else if (interactive)
                    {
                        await client.InteractiveAuthenticationAsync();
                    }
                    else
                    {
                        Console.WriteLine("Authentication required. Use --token, --username/--password, or --interactive");
                        return;
                    }

                    // We execute our test suite based on the selected mode
                    if (interactive)
                    {
                        await client.RunInteractiveModeAsync();
                    }
                    else
                    {
                        await client.RunAutomatedTestsAsync(outputFile);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Test execution failed: {ex.Message}");
                    if (verbose)
                    {
                        Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    }
                }
            });

            return await rootCommand.InvokeAsync(args);
        }
    }

    /// <summary>
    /// We implement our comprehensive API test client with full endpoint coverage.
    /// Our design includes authentication management, error handling, and result formatting.
    /// </summary>
    public class ApiTestClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly bool _verbose;
        private string? _bearerToken;

        /// <summary>
        /// We initialize our test client with base configuration and HTTP settings.
        /// Our constructor sets up proper SSL handling and timeout configurations.
        /// </summary>
        public ApiTestClient(string baseUrl, bool verbose = false)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _verbose = verbose;
            
            // We configure our HTTP client for API testing
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            
            // We configure SSL certificate handling for development environments
            var handler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            
            _httpClient = new HttpClient(handler);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// We provide interactive authentication with user input prompts.
        /// Our implementation securely handles password input and validates credentials.
        /// </summary>
        public async Task InteractiveAuthenticationAsync()
        {
            Console.Write("Username: ");
            var username = Console.ReadLine();
            
            Console.Write("Password: ");
            var password = ReadPasswordFromConsole();
            
            await AuthenticateAsync(username!, password);
        }

        /// <summary>
        /// We implement secure password reading from console input.
        /// Our approach hides password characters while maintaining usability.
        /// </summary>
        private static string ReadPasswordFromConsole()
        {
            var password = new StringBuilder();
            ConsoleKeyInfo keyInfo;
            
            do
            {
                keyInfo = Console.ReadKey(true);
                
                if (keyInfo.Key != ConsoleKey.Backspace && keyInfo.Key != ConsoleKey.Enter)
                {
                    password.Append(keyInfo.KeyChar);
                    Console.Write("*");
                }
                else if (keyInfo.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password.Remove(password.Length - 1, 1);
                    Console.Write("\b \b");
                }
            }
            while (keyInfo.Key != ConsoleKey.Enter);
            
            Console.WriteLine();
            return password.ToString();
        }

        /// <summary>
        /// We authenticate with the API using username and password credentials.
        /// Our method handles JWT token extraction and storage for subsequent requests.
        /// </summary>
        public async Task AuthenticateAsync(string username, string password)
        {
            WriteVerbose($"Authenticating user: {username}");
            
            var loginRequest = new
            {
                Username = username,
                Password = password
            };

            var json = JsonSerializer.Serialize(loginRequest, jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync($"{_baseUrl}/api/v1/auth/login", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var authResult = JsonSerializer.Deserialize<AuthenticationResult>(responseJson, jsonOptions);
                    
                    _bearerToken = authResult?.Token;
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _bearerToken);
                    
                    Console.WriteLine("Authentication successful!");
                    WriteVerbose($"Bearer token: {_bearerToken?[..20]}...");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Authentication failed: {response.StatusCode} - {errorContent}");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Network error during authentication: {ex.Message}");
            }
        }

        /// <summary>
        /// We set a bearer token directly for API authentication.
        /// Our method validates token format and configures HTTP headers appropriately.
        /// </summary>
        public async Task SetBearerTokenAsync(string token)
        {
            _bearerToken = token;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            // We validate the token by making a test request
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/v1/systemmetrics/current");
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Token validation failed: {response.StatusCode}");
                }
                
                Console.WriteLine("Bearer token set successfully!");
            }
            catch (Exception ex)
            {
                throw new Exception($"Token validation error: {ex.Message}");
            }
        }

        /// <summary>
        /// We execute our comprehensive automated test suite covering all API endpoints.
        /// Our implementation provides detailed reporting and error tracking.
        /// </summary>
        public async Task RunAutomatedTestsAsync(string? outputFile = null)
        {
            var testResults = new List<TestResult>();
            
            Console.WriteLine("Running automated API tests...\n");

            // We test system metrics endpoints
            testResults.Add(await TestEndpoint("GET", "/api/v1/systemmetrics/current", "Get Current System Metrics"));
            testResults.Add(await TestEndpoint("GET", "/api/v1/systemmetrics/summary?hours=24", "Get System Metrics Summary"));
            testResults.Add(await TestEndpoint("GET", "/api/v1/systemmetrics/alerts", "Get Active Alerts"));
            
            // We test historical metrics with date parameters
            var startTime = DateTime.UtcNow.AddHours(-1).ToString("yyyy-MM-ddTHH:mm:ssZ");
            var endTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            testResults.Add(await TestEndpoint("GET", 
                $"/api/v1/systemmetrics/historical?startTime={startTime}&endTime={endTime}", 
                "Get Historical Metrics"));

            // We test Kubernetes endpoints
            testResults.Add(await TestEndpoint("GET", "/api/v1/kubernetes/cluster/status", "Get Kubernetes Cluster Status"));
            testResults.Add(await TestEndpoint("GET", "/api/v1/kubernetes/pods", "Get Kubernetes Pods"));

            // We generate our test report
            GenerateTestReport(testResults, outputFile);
        }

        /// <summary>
        /// We provide an interactive mode for manual API testing and exploration.
        /// Our implementation offers a menu-driven interface for comprehensive testing.
        /// </summary>
        public async Task RunInteractiveModeAsync()
        {
            Console.WriteLine("=== BTHL CheckGate API Test Client - Interactive Mode ===\n");

            while (true)
            {
                Console.WriteLine("Available test options:");
                Console.WriteLine("1. Get Current System Metrics");
                Console.WriteLine("2. Get System Metrics Summary");
                Console.WriteLine("3. Get Historical Metrics");
                Console.WriteLine("4. Get Active Alerts");
                Console.WriteLine("5. Get Kubernetes Cluster Status");
                Console.WriteLine("6. Get Kubernetes Pods");
                Console.WriteLine("7. Run All Tests");
                Console.WriteLine("8. Custom Request");
                Console.WriteLine("9. Exit");
                Console.Write("\nSelect an option (1-9): ");

                var choice = Console.ReadLine();

                try
                {
                    switch (choice)
                    {
                        case "1":
                            await ExecuteTest("/api/v1/systemmetrics/current", "Current System Metrics");
                            break;
                        case "2":
                            await ExecuteTest("/api/v1/systemmetrics/summary?hours=24", "System Metrics Summary");
                            break;
                        case "3":
                            await GetHistoricalMetricsInteractive();
                            break;
                        case "4":
                            await ExecuteTest("/api/v1/systemmetrics/alerts", "Active Alerts");
                            break;
                        case "5":
                            await ExecuteTest("/api/v1/kubernetes/cluster/status", "Kubernetes Cluster Status");
                            break;
                        case "6":
                            await ExecuteTest("/api/v1/kubernetes/pods", "Kubernetes Pods");
                            break;
                        case "7":
                            await RunAutomatedTestsAsync();
                            break;
                        case "8":
                            await CustomRequestInteractive();
                            break;
                        case "9":
                            Console.WriteLine("Exiting interactive mode...");
                            return;
                        default:
                            Console.WriteLine("Invalid option. Please try again.\n");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error executing request: {ex.Message}\n");
                }

                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                Console.Clear();
            }
        }

        /// <summary>
        /// We handle interactive historical metrics requests with user-specified time ranges.
        /// Our implementation provides date/time input validation and formatting assistance.
        /// </summary>
        private async Task GetHistoricalMetricsInteractive()
        {
            Console.Write("Enter start time (yyyy-MM-dd HH:mm or leave blank for 1 hour ago): ");
            var startInput = Console.ReadLine();
            
            Console.Write("Enter end time (yyyy-MM-dd HH:mm or leave blank for now): ");
            var endInput = Console.ReadLine();

            DateTime startTime = string.IsNullOrEmpty(startInput) 
                ? DateTime.UtcNow.AddHours(-1) 
                : DateTime.Parse(startInput).ToUniversalTime();
                
            DateTime endTime = string.IsNullOrEmpty(endInput) 
                ? DateTime.UtcNow 
                : DateTime.Parse(endInput).ToUniversalTime();

            var url = $"/api/v1/systemmetrics/historical?startTime={startTime:yyyy-MM-ddTHH:mm:ssZ}&endTime={endTime:yyyy-MM-ddTHH:mm:ssZ}";
            await ExecuteTest(url, "Historical Metrics");
        }

        /// <summary>
        /// We provide custom request capabilities for testing arbitrary endpoints.
        /// Our implementation includes method selection and request body composition.
        /// </summary>
        private async Task CustomRequestInteractive()
        {
            Console.Write("Enter HTTP method (GET, POST, PUT, DELETE): ");
            var method = Console.ReadLine()?.ToUpper() ?? "GET";
            
            Console.Write("Enter endpoint path (starting with /): ");
            var path = Console.ReadLine() ?? "/";
            
            if (method != "GET")
            {
                Console.Write("Enter request body (JSON, or leave blank): ");
                var body = Console.ReadLine();
                await ExecuteTest(path, $"Custom {method} Request", method, body);
            }
            else
            {
                await ExecuteTest(path, $"Custom {method} Request", method);
            }
        }

        /// <summary>
        /// We execute individual API tests with comprehensive error handling and result formatting.
        /// Our method provides detailed response information for debugging and validation.
        /// </summary>
        private async Task ExecuteTest(string endpoint, string description, string method = "GET", string? body = null)
        {
            Console.WriteLine($"\n=== {description} ===");
            Console.WriteLine($"Endpoint: {method} {endpoint}");
            
            try
            {
                HttpResponseMessage response;
                
                if (method == "GET")
                {
                    response = await _httpClient.GetAsync($"{_baseUrl}{endpoint}");
                }
                else
                {
                    var content = !string.IsNullOrEmpty(body) 
                        ? new StringContent(body, Encoding.UTF8, "application/json")
                        : new StringContent("", Encoding.UTF8, "application/json");
                        
                    response = method switch
                    {
                        "POST" => await _httpClient.PostAsync($"{_baseUrl}{endpoint}", content),
                        "PUT" => await _httpClient.PutAsync($"{_baseUrl}{endpoint}", content),
                        "DELETE" => await _httpClient.DeleteAsync($"{_baseUrl}{endpoint}"),
                        _ => throw new ArgumentException($"Unsupported HTTP method: {method}")
                    };
                }

                Console.WriteLine($"Status: {(int)response.StatusCode} {response.StatusCode}");
                
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Response:");
                    
                    // We format JSON responses for better readability
                    try
                    {
                        var jsonDoc = JsonDocument.Parse(responseContent);
                        var formattedJson = JsonSerializer.Serialize(jsonDoc, jsonOptions);
                        Console.WriteLine(formattedJson);
                    }
                    catch
                    {
                        Console.WriteLine(responseContent);
                    }
                }
                else
                {
                    Console.WriteLine($"Error: {responseContent}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Request failed: {ex.Message}");
            }
        }

        /// <summary>
        /// We test individual endpoints and return structured results for reporting.
        /// Our method captures timing, status, and error information for analysis.
        /// </summary>
        private async Task<TestResult> TestEndpoint(string method, string endpoint, string description)
        {
            var startTime = DateTime.UtcNow;
            var result = new TestResult
            {
                Method = method,
                Endpoint = endpoint,
                Description = description,
                StartTime = startTime
            };

            try
            {
                WriteVerbose($"Testing: {method} {endpoint}");
                
                var response = await _httpClient.GetAsync($"{_baseUrl}{endpoint}");
                
                result.StatusCode = (int)response.StatusCode;
                result.Success = response.IsSuccessStatusCode;
                result.ResponseTime = DateTime.UtcNow - startTime;
                
                if (response.IsSuccessStatusCode)
                {
                    result.ResponseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"✓ {description} - {response.StatusCode} ({result.ResponseTime.TotalMilliseconds:F0}ms)");
                }
                else
                {
                    result.ErrorMessage = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"✗ {description} - {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                result.ResponseTime = DateTime.UtcNow - startTime;
                Console.WriteLine($"✗ {description} - Exception: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// We generate comprehensive test reports with detailed results and statistics.
        /// Our reporting includes success rates, timing information, and error analysis.
        /// </summary>
        private void GenerateTestReport(List<TestResult> results, string? outputFile = null)
        {
            var report = new StringBuilder();
            var successCount = results.Count(r => r.Success);
            var totalCount = results.Count;
            var averageResponseTime = results.Where(r => r.Success).Average(r => r.ResponseTime.TotalMilliseconds);

            report.AppendLine("=== BTHL CheckGate API Test Report ===");
            report.AppendLine($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            report.AppendLine($"Total Tests: {totalCount}");
            report.AppendLine($"Successful: {successCount}");
            report.AppendLine($"Failed: {totalCount - successCount}");
            report.AppendLine($"Success Rate: {(double)successCount / totalCount * 100:F1}%");
            report.AppendLine($"Average Response Time: {averageResponseTime:F0}ms");
            report.AppendLine();

            report.AppendLine("=== Test Results ===");
            foreach (var result in results)
            {
                report.AppendLine($"{(result.Success ? "✓" : "✗")} {result.Description}");
                report.AppendLine($"   {result.Method} {result.Endpoint}");
                report.AppendLine($"   Status: {result.StatusCode}");
                report.AppendLine($"   Response Time: {result.ResponseTime.TotalMilliseconds:F0}ms");
                
                if (!result.Success && !string.IsNullOrEmpty(result.ErrorMessage))
                {
                    report.AppendLine($"   Error: {result.ErrorMessage}");
                }
                report.AppendLine();
            }

            var reportContent = report.ToString();
            Console.WriteLine(reportContent);

            // We save the report to file if specified
            if (!string.IsNullOrEmpty(outputFile))
            {
                File.WriteAllText(outputFile, reportContent);
                Console.WriteLine($"Test report saved to: {outputFile}");
            }
        }

        /// <summary>
        /// We output verbose messages when detailed logging is enabled.
        /// Our implementation provides additional debugging information for troubleshooting.
        /// </summary>
        private void WriteVerbose(string message)
        {
            if (_verbose)
            {
                Console.WriteLine($"[VERBOSE] {message}");
            }
        }
    }

    #region Data Models

    /// <summary>
    /// We represent authentication results from the API login endpoint.
    /// Our model captures the JWT token and user information returned by the API.
    /// </summary>
    public class AuthenticationResult
    {
        public string? Token { get; set; }
        public UserInfo? User { get; set; }
    }

    /// <summary>
    /// We structure user information for authentication responses.
    /// Our design includes essential user details needed for session management.
    /// </summary>
    public class UserInfo
    {
        public string? Username { get; set; }
        public List<string>? Roles { get; set; }
    }

    /// <summary>
    /// We capture comprehensive test result information for reporting and analysis.
    /// Our structure includes timing, status, and error details for each test execution.
    /// </summary>
    public class TestResult
    {
        public string Method { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public TimeSpan ResponseTime { get; set; }
        public int StatusCode { get; set; }
        public bool Success { get; set; }
        public string? ResponseContent { get; set; }
        public string? ErrorMessage { get; set; }
    }

    #endregion
}
