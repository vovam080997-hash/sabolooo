using System.Text;
using CinemaGo.Application.Abstractions;
using CinemaGo.Application.Services;
using CinemaGo.ConsoleApp.Configuration;
using CinemaGo.ConsoleApp.Menus;
using CinemaGo.Infrastructure.Email;
using CinemaGo.Infrastructure.Export;
using CinemaGo.Infrastructure.Halls;
using CinemaGo.Infrastructure.Logging;
using CinemaGo.Infrastructure.Persistence;

Console.OutputEncoding = Encoding.UTF8;



AppSettings settings = AppSettingsLoader.Load(); 

IAppLogger logger = new FileAppLogger();

IUserRepository userRepository = new TextFileUserRepository(logger);
IMovieRepository movieRepository = new TextFileMovieRepository(logger);
ISessionRepository sessionRepository = new TextFileSessionRepository(logger);
IBookingRepository bookingRepository = new TextFileBookingRepository(logger);
IHallRepository hallRepository = new InMemoryHallRepository();
IEmailSender emailSender = new SmtpEmailSender(
    logger,
    smtpHost: settings.Smtp.Host,
    smtpPort: settings.Smtp.Port,
    smtpUser: settings.Smtp.User,
    smtpPassword: settings.Smtp.Password,
    enabled: settings.Smtp.Enabled);
IDataExporter exporter = new CsvFileExporter(logger);

var seatMapService = new SeatMapService(bookingRepository, hallRepository);
var statusUpdater = new SessionStatusUpdater(sessionRepository, movieRepository, logger);
var authService = new AuthService(userRepository, logger);
var catalogService = new CatalogService(sessionRepository, movieRepository, seatMapService);
var bookingService = new BookingService(sessionRepository, movieRepository, bookingRepository, emailSender, logger, seatMapService);
var adminService = new AdminService(userRepository, movieRepository, sessionRepository, bookingRepository, hallRepository, logger, exporter, seatMapService);

var customerMenu = new CustomerMenu(authService, catalogService, bookingService);
var adminMenu = new AdminMenu(authService, adminService, catalogService, customerMenu);
var app = new AppRunner(authService, statusUpdater, adminMenu, customerMenu);

app.Run();
