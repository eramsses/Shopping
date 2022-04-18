using Shopping.Data.Entities;
using Shopping.Enums;
using Shopping.Helpers;

namespace Shopping.Data
{
    public class SeedDb
    {
        private DataContext _context;
        private readonly IUserHelper _userHelper;

        public SeedDb(DataContext context, IUserHelper userHelper)
        {
            _context = context;
            _userHelper = userHelper;
        }

        public async Task SeedAsync()
        {
            await _context.Database.EnsureCreatedAsync();
            await CheckCategories();
            await CheckCountries();
            await CheckRolesAsync();
            await CheckUserAsync("1010", "Erick", "Rodriguez", "erick@yopmail.com", "+50432942101", "Valle de Ángeles", UserType.Admin);
            await CheckUserAsync("2020", "Xiomara", "Bustillo", "xiomara@yopmail.com", "+50499944709", "Valle de Ángeles", UserType.User);

        }

        private async Task<User> CheckUserAsync(string document, string firstName, string lastName, string email, string phone, string address, UserType userType)
        {
            User user = await _userHelper.GetUserAsync(email);
            if (user == null)
            {
                user = new User
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    UserName = email,
                    PhoneNumber = phone,
                    Address = address,
                    Document = document,
                    City = _context.Cities.FirstOrDefault(),
                    UserType = userType,
                };

                await _userHelper.AddUserAsync(user, "123456");
                await _userHelper.AddUserToRoleAsync(user, userType.ToString());

                //Autoconfirmar al usuario autocreado
                string token = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
                await _userHelper.ConfirmEmailAsync(user, token);

            }

            return user;

        }

        private async Task CheckRolesAsync()
        {
            await _userHelper.CheckRoleAsync(UserType.Admin.ToString());
            await _userHelper.CheckRoleAsync(UserType.User.ToString());

        }

        private async Task CheckCountries()
        {
            if (!_context.Countries.Any())
            {
                _context.Countries.Add(
                    new Country
                    {
                        Name = "Honduras",
                        States = new List<State>()
                    {
                        new State {
                            Name = "Francisco Morazan",
                            Cities = new List<City>()
                            {
                                new City{Name = "Tegucigalpa"},
                                new City{Name = "Comayaguela"},
                                new City{Name = "Valle de Ángeles"},
                                new City{Name = "Santa Lucía"},
                                new City{Name = "Talanga"},
                                new City{Name = "Orica"},
                                new City{Name = "Ojojona"},
                            }
                        },
                        new State {
                            Name = "Cortés",
                            Cities = new List<City>()
                            {
                                new City{Name = "San Pedro Sula"},
                                new City{Name = "La Lima"},
                                new City{Name = "Choloma"},
                                new City{Name = "Puerto Cortés"},
                                new City{Name = "Villa Nueva"},
                                new City{Name = "Potrerillos"},

                            }
                        }
                    },


                    }
                );

                _context.Countries.Add(
                    new Country
                    {
                        Name = "Colombia",
                        States = new List<State>()
                    {
                        new State {
                            Name = "Antioquia",
                            Cities = new List<City>()
                            {
                                new City{Name = "Medellín"},
                                new City{Name = "Itaguí"},
                                new City{Name = "Envigado"},
                                new City{Name = "Bello"},
                                new City{Name = "Rionegro"},
                            }
                        },
                        new State {
                            Name = "Bogotá",
                            Cities = new List<City>()
                            {
                                new City{Name = "Isaquen"},
                                new City{Name = "Chapinero"},
                                new City{Name = "Santa Fe"},
                                new City{Name = "Puerto Cortés"},
                                new City{Name = "Useme"},
                                new City{Name = "Bosa"},

                            }
                        }
                    },


                    }
                );

                _context.Countries.Add(
                    new Country
                    {
                        Name = "Estados Unidos",
                        States = new List<State>()
                    {
                        new State {
                            Name = "Florida",
                            Cities = new List<City>()
                            {
                                new City{Name = "Orlando"},
                                new City{Name = "Miami"},
                                new City{Name = "Tampa"},
                                new City{Name = "Fort Louderdale"},
                                new City{Name = "Key West"},
                            }
                        },
                        new State {
                            Name = "Texas",
                            Cities = new List<City>()
                            {
                                new City{Name = "Houston"},
                                new City{Name = "San Antonio"},
                                new City{Name = "Dallas"},
                                new City{Name = "Austin"},
                                new City{Name = "El Paso"},

                            }
                        }
                    },


                    }
                );

                await _context.SaveChangesAsync();

            }
        }

        private async Task CheckCategories()
        {
            if (!_context.Categories.Any())
            {
                _context.Categories.Add(new Category { Name = "Tecnología" });
                _context.Categories.Add(new Category { Name = "Ropa" });
                _context.Categories.Add(new Category { Name = "Calzado" });
                _context.Categories.Add(new Category { Name = "Belleza" });
                _context.Categories.Add(new Category { Name = "Nutrición" });
                _context.Categories.Add(new Category { Name = "Deportes" });
                _context.Categories.Add(new Category { Name = "Apple" });
                _context.Categories.Add(new Category { Name = "Mascotas" });

                await _context.SaveChangesAsync();
            }
        }
    }
}
