using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Invenio.Core;
using Invenio.Core.Data;
using Invenio.Core.Domain;
using Invenio.Core.Domain.Catalog;
using Invenio.Core.Domain.Common;
using Invenio.Core.Domain.Users;
using Invenio.Core.Domain.Directory;
using Invenio.Core.Domain.Localization;
using Invenio.Core.Domain.Logging;
using Invenio.Core.Domain.Media;
using Invenio.Core.Domain.Messages;
using Invenio.Core.Domain.Security;
using Invenio.Core.Domain.Stores;
using Invenio.Core.Domain.Tasks;
using Invenio.Core.Infrastructure;
using Invenio.Services.Common;
using Invenio.Services.Configuration;
using Invenio.Services.Users;
using Invenio.Services.Helpers;
using Invenio.Services.Localization;
using Invenio.Services.Media;

namespace Invenio.Services.Installation
{
    public partial class CodeFirstInstallationService : IInstallationService
    {
        #region Fields

        private readonly IRepository<Store> _storeRepository;
        private readonly IRepository<MeasureDimension> _measureDimensionRepository;
        private readonly IRepository<MeasureWeight> _measureWeightRepository;
        private readonly IRepository<Language> _languageRepository;
        private readonly IRepository<Currency> _currencyRepository;
        private readonly IRepository<User> _UserRepository;
        private readonly IRepository<UserPassword> _UserPasswordRepository;
        private readonly IRepository<UserRole> _UserRoleRepository;
        private readonly IRepository<EmailAccount> _emailAccountRepository;
        private readonly IRepository<MessageTemplate> _messageTemplateRepository;
        private readonly IRepository<Country> _countryRepository;
        private readonly IRepository<StateProvince> _stateProvinceRepository;
        private readonly IRepository<ActivityLogType> _activityLogTypeRepository;
        private readonly IRepository<ActivityLog> _activityLogRepository;
        private readonly IRepository<ScheduleTask> _scheduleTaskRepository;
        private readonly IRepository<Address> _addressRepository;
        private readonly IRepository<SearchTerm> _searchTermRepository;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IWebHelper _webHelper;

        #endregion

        #region Ctor

        public CodeFirstInstallationService(IRepository<Store> storeRepository,
            IRepository<MeasureDimension> measureDimensionRepository,
            IRepository<MeasureWeight> measureWeightRepository,
            IRepository<Language> languageRepository,
            IRepository<Currency> currencyRepository,
            IRepository<User> UserRepository,
            IRepository<UserPassword> UserPasswordRepository,
            IRepository<UserRole> UserRoleRepository,
            IRepository<EmailAccount> emailAccountRepository,
            IRepository<MessageTemplate> messageTemplateRepository,
            IRepository<Country> countryRepository,
            IRepository<StateProvince> stateProvinceRepository,
            IRepository<ActivityLogType> activityLogTypeRepository,
            IRepository<ActivityLog> activityLogRepository,
            IRepository<ScheduleTask> scheduleTaskRepository,
            IRepository<Address> addressRepository,
            IRepository<SearchTerm> searchTermRepository,
            IGenericAttributeService genericAttributeService,
            IWebHelper webHelper)
        {
            this._storeRepository = storeRepository;
            this._measureDimensionRepository = measureDimensionRepository;
            this._measureWeightRepository = measureWeightRepository;
            this._languageRepository = languageRepository;
            this._currencyRepository = currencyRepository;
            this._UserRepository = UserRepository;
            this._UserPasswordRepository = UserPasswordRepository;
            this._UserRoleRepository = UserRoleRepository;
            this._emailAccountRepository = emailAccountRepository;
            this._messageTemplateRepository = messageTemplateRepository;
            this._countryRepository = countryRepository;
            this._stateProvinceRepository = stateProvinceRepository;
            this._activityLogTypeRepository = activityLogTypeRepository;
            this._activityLogRepository = activityLogRepository;
            this._scheduleTaskRepository = scheduleTaskRepository;
            this._addressRepository = addressRepository;
            this._searchTermRepository = searchTermRepository;
            this._genericAttributeService = genericAttributeService;
            this._webHelper = webHelper;
        }

        #endregion

        #region Utilities

        protected virtual void InstallStores()
        {
            //var storeUrl = "http://www.yourStore.com/";
            var storeUrl = _webHelper.GetStoreLocation(false);
            var stores = new List<Store>
            {
                new Store
                {
                    Name = "Your store name",
                    Url = storeUrl,
                    SslEnabled = false,
                    Hosts = "yourstore.com,www.yourstore.com",
                    DisplayOrder = 1,
                    //should we set some default company info?
                    CompanyName = "Your company name",
                    CompanyAddress = "your company country, state, zip, street, etc",
                    CompanyPhoneNumber = "(123) 456-78901",
                    CompanyVat = null,
                },
            };

            _storeRepository.Insert(stores);
        }

        protected virtual void InstallMeasures()
        {
            var measureDimensions = new List<MeasureDimension>
            {
                new MeasureDimension
                {
                    Name = "inch(es)",
                    SystemKeyword = "inches",
                    Ratio = 1M,
                    DisplayOrder = 1,
                },
                new MeasureDimension
                {
                    Name = "feet",
                    SystemKeyword = "feet",
                    Ratio = 0.08333333M,
                    DisplayOrder = 2,
                },
                new MeasureDimension
                {
                    Name = "meter(s)",
                    SystemKeyword = "meters",
                    Ratio = 0.0254M,
                    DisplayOrder = 3,
                },
                new MeasureDimension
                {
                    Name = "millimetre(s)",
                    SystemKeyword = "millimetres",
                    Ratio = 25.4M,
                    DisplayOrder = 4,
                }
            };

            _measureDimensionRepository.Insert(measureDimensions);

            var measureWeights = new List<MeasureWeight>
            {
                new MeasureWeight
                {
                    Name = "ounce(s)",
                    SystemKeyword = "ounce",
                    Ratio = 16M,
                    DisplayOrder = 1,
                },
                new MeasureWeight
                {
                    Name = "lb(s)",
                    SystemKeyword = "lb",
                    Ratio = 1M,
                    DisplayOrder = 2,
                },
                new MeasureWeight
                {
                    Name = "kg(s)",
                    SystemKeyword = "kg",
                    Ratio = 0.45359237M,
                    DisplayOrder = 3,
                },
                new MeasureWeight
                {
                    Name = "gram(s)",
                    SystemKeyword = "grams",
                    Ratio = 453.59237M,
                    DisplayOrder = 4,
                }
            };

            _measureWeightRepository.Insert(measureWeights);
        }
        
        protected virtual void InstallLanguages()
        {
            var language = new Language
            {
                Name = "English",
                LanguageCulture = "en-US",
                UniqueSeoCode = "en",
                FlagImageFileName = "us.png",
                Published = true,
                DisplayOrder = 1
            };
            _languageRepository.Insert(language);
        }

        protected virtual void InstallLocaleResources()
        {
            //'English' language
            var language = _languageRepository.Table.Single(l => l.Name == "English");

            //save resources
            foreach (var filePath in System.IO.Directory.EnumerateFiles(CommonHelper.MapPath("~/App_Data/Localization/"), "*.nopres.xml", SearchOption.TopDirectoryOnly))
            {
                var localesXml = File.ReadAllText(filePath);
                var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
                localizationService.ImportResourcesFromXml(language, localesXml);
            }

        }

        protected virtual void InstallCurrencies()
        {
            var currencies = new List<Currency>
            {
                new Currency
                {
                    Name = "US Dollar",
                    CurrencyCode = "USD",
                    Rate = 1,
                    DisplayLocale = "en-US",
                    CustomFormatting = "",
                    Published = true,
                    DisplayOrder = 1,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                    RoundingType = RoundingType.Rounding001
                },
                new Currency
                {
                    Name = "Australian Dollar",
                    CurrencyCode = "AUD",
                    Rate = 1.36M,
                    DisplayLocale = "en-AU",
                    CustomFormatting = "",
                    Published = false,
                    DisplayOrder = 2,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                    RoundingType = RoundingType.Rounding001
                },
                new Currency
                {
                    Name = "British Pound",
                    CurrencyCode = "GBP",
                    Rate = 0.82M,
                    DisplayLocale = "en-GB",
                    CustomFormatting = "",
                    Published = false,
                    DisplayOrder = 3,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                    RoundingType = RoundingType.Rounding001
                },
                new Currency
                {
                    Name = "Canadian Dollar",
                    CurrencyCode = "CAD",
                    Rate = 1.32M,
                    DisplayLocale = "en-CA",
                    CustomFormatting = "",
                    Published = false,
                    DisplayOrder = 4,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                    RoundingType = RoundingType.Rounding001
                },
                new Currency
                {
                    Name = "Chinese Yuan Renminbi",
                    CurrencyCode = "CNY",
                    Rate = 6.93M,
                    DisplayLocale = "zh-CN",
                    CustomFormatting = "",
                    Published = false,
                    DisplayOrder = 5,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                    RoundingType = RoundingType.Rounding001
                },
                new Currency
                {
                    Name = "Euro",
                    CurrencyCode = "EUR",
                    Rate = 0.95M,
                    DisplayLocale = "",
                    //CustomFormatting = "ˆ0.00",
                    CustomFormatting = string.Format("{0}0.00", "\u20ac"),
                    Published = true,
                    DisplayOrder = 6,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                    RoundingType = RoundingType.Rounding001
                },
                new Currency
                {
                    Name = "Hong Kong Dollar",
                    CurrencyCode = "HKD",
                    Rate = 7.75M,
                    DisplayLocale = "zh-HK",
                    CustomFormatting = "",
                    Published = false,
                    DisplayOrder = 7,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                    RoundingType = RoundingType.Rounding001
                },
                new Currency
                {
                    Name = "Japanese Yen",
                    CurrencyCode = "JPY",
                    Rate = 116.64M,
                    DisplayLocale = "ja-JP",
                    CustomFormatting = "",
                    Published = false,
                    DisplayOrder = 8,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                    RoundingType = RoundingType.Rounding001
                },
                new Currency
                {
                    Name = "Russian Rouble",
                    CurrencyCode = "RUB",
                    Rate = 59.75M,
                    DisplayLocale = "ru-RU",
                    CustomFormatting = "",
                    Published = false,
                    DisplayOrder = 9,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                    RoundingType = RoundingType.Rounding001
                },
                new Currency
                {
                    Name = "Swedish Krona",
                    CurrencyCode = "SEK",
                    Rate = 9.08M,
                    DisplayLocale = "sv-SE",
                    CustomFormatting = "",
                    Published = false,
                    DisplayOrder = 10,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                    RoundingType = RoundingType.Rounding1
                },
                new Currency
                {
                    Name = "Romanian Leu",
                    CurrencyCode = "RON",
                    Rate = 4.28M,
                    DisplayLocale = "ro-RO",
                    CustomFormatting = "",
                    Published = false,
                    DisplayOrder = 11,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                    RoundingType = RoundingType.Rounding001
                },
                new Currency
                {
                    Name = "Indian Rupee",
                    CurrencyCode = "INR",
                    Rate = 68.17M,
                    DisplayLocale = "en-IN",
                    CustomFormatting = "",
                    Published = false,
                    DisplayOrder = 12,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                    RoundingType = RoundingType.Rounding001
                },
            };
            _currencyRepository.Insert(currencies);
        }

        protected virtual void InstallCountriesAndStates()
        {
            var cUsa = new Country
            {
                Name = "United States",
                AllowsBilling = true,
                AllowsShipping = true,
                TwoLetterIsoCode = "US",
                ThreeLetterIsoCode = "USA",
                NumericIsoCode = 840,
                SubjectToVat = false,
                DisplayOrder = 1,
                Published = false,
            };
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "AA (Armed Forces Americas)",
            //    Abbreviation = "AA",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "AE (Armed Forces Europe)",
            //    Abbreviation = "AE",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Alabama",
            //    Abbreviation = "AL",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Alaska",
            //    Abbreviation = "AK",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "American Samoa",
            //    Abbreviation = "AS",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "AP (Armed Forces Pacific)",
            //    Abbreviation = "AP",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Arizona",
            //    Abbreviation = "AZ",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Arkansas",
            //    Abbreviation = "AR",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "California",
            //    Abbreviation = "CA",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Colorado",
            //    Abbreviation = "CO",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Connecticut",
            //    Abbreviation = "CT",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Delaware",
            //    Abbreviation = "DE",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "District of Columbia",
            //    Abbreviation = "DC",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Federated States of Micronesia",
            //    Abbreviation = "FM",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Florida",
            //    Abbreviation = "FL",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Georgia",
            //    Abbreviation = "GA",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Guam",
            //    Abbreviation = "GU",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Hawaii",
            //    Abbreviation = "HI",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Idaho",
            //    Abbreviation = "ID",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Illinois",
            //    Abbreviation = "IL",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Indiana",
            //    Abbreviation = "IN",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Iowa",
            //    Abbreviation = "IA",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Kansas",
            //    Abbreviation = "KS",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Kentucky",
            //    Abbreviation = "KY",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Louisiana",
            //    Abbreviation = "LA",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Maine",
            //    Abbreviation = "ME",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Marshall Islands",
            //    Abbreviation = "MH",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Maryland",
            //    Abbreviation = "MD",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Massachusetts",
            //    Abbreviation = "MA",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Michigan",
            //    Abbreviation = "MI",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Minnesota",
            //    Abbreviation = "MN",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Mississippi",
            //    Abbreviation = "MS",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Missouri",
            //    Abbreviation = "MO",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Montana",
            //    Abbreviation = "MT",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Nebraska",
            //    Abbreviation = "NE",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Nevada",
            //    Abbreviation = "NV",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "New Hampshire",
            //    Abbreviation = "NH",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "New Jersey",
            //    Abbreviation = "NJ",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "New Mexico",
            //    Abbreviation = "NM",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "New York",
            //    Abbreviation = "NY",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "North Carolina",
            //    Abbreviation = "NC",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "North Dakota",
            //    Abbreviation = "ND",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Northern Mariana Islands",
            //    Abbreviation = "MP",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Ohio",
            //    Abbreviation = "OH",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Oklahoma",
            //    Abbreviation = "OK",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Oregon",
            //    Abbreviation = "OR",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Palau",
            //    Abbreviation = "PW",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Pennsylvania",
            //    Abbreviation = "PA",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Puerto Rico",
            //    Abbreviation = "PR",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Rhode Island",
            //    Abbreviation = "RI",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "South Carolina",
            //    Abbreviation = "SC",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "South Dakota",
            //    Abbreviation = "SD",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Tennessee",
            //    Abbreviation = "TN",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Texas",
            //    Abbreviation = "TX",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Utah",
            //    Abbreviation = "UT",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Vermont",
            //    Abbreviation = "VT",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Virgin Islands",
            //    Abbreviation = "VI",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Virginia",
            //    Abbreviation = "VA",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Washington",
            //    Abbreviation = "WA",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "West Virginia",
            //    Abbreviation = "WV",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Wisconsin",
            //    Abbreviation = "WI",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cUsa.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Wyoming",
            //    Abbreviation = "WY",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //var cCanada = new Country
            //{
            //    Name = "Canada",
            //    AllowsBilling = true,
            //    AllowsShipping = true,
            //    TwoLetterIsoCode = "CA",
            //    ThreeLetterIsoCode = "CAN",
            //    NumericIsoCode = 124,
            //    SubjectToVat = false,
            //    DisplayOrder = 100,
            //    Published = false,
            //};
            //cCanada.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Alberta",
            //    Abbreviation = "AB",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cCanada.StateProvinces.Add(new StateProvince
            //{
            //    Name = "British Columbia",
            //    Abbreviation = "BC",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cCanada.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Manitoba",
            //    Abbreviation = "MB",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cCanada.StateProvinces.Add(new StateProvince
            //{
            //    Name = "New Brunswick",
            //    Abbreviation = "NB",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cCanada.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Newfoundland and Labrador",
            //    Abbreviation = "NL",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cCanada.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Northwest Territories",
            //    Abbreviation = "NT",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cCanada.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Nova Scotia",
            //    Abbreviation = "NS",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cCanada.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Nunavut",
            //    Abbreviation = "NU",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cCanada.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Ontario",
            //    Abbreviation = "ON",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cCanada.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Prince Edward Island",
            //    Abbreviation = "PE",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cCanada.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Quebec",
            //    Abbreviation = "QC",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cCanada.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Saskatchewan",
            //    Abbreviation = "SK",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            //cCanada.StateProvinces.Add(new StateProvince
            //{
            //    Name = "Yukon Territory",
            //    Abbreviation = "YT",
            //    Published = true,
            //    DisplayOrder = 1,
            //});
            var countries = new List<Country>
                                {
                                    cUsa,
                                    //cCanada,
                                    //other countries
                                    new Country
                                    {
                                        Name = "Argentina",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "AR",
                                        ThreeLetterIsoCode = "ARG",
                                        NumericIsoCode = 32,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Armenia",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "AM",
                                        ThreeLetterIsoCode = "ARM",
                                        NumericIsoCode = 51,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Aruba",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "AW",
                                        ThreeLetterIsoCode = "ABW",
                                        NumericIsoCode = 533,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Australia",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "AU",
                                        ThreeLetterIsoCode = "AUS",
                                        NumericIsoCode = 36,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Austria",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "AT",
                                        ThreeLetterIsoCode = "AUT",
                                        NumericIsoCode = 40,
                                        SubjectToVat = true,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Azerbaijan",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "AZ",
                                        ThreeLetterIsoCode = "AZE",
                                        NumericIsoCode = 31,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Bahamas",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "BS",
                                        ThreeLetterIsoCode = "BHS",
                                        NumericIsoCode = 44,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Bangladesh",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "BD",
                                        ThreeLetterIsoCode = "BGD",
                                        NumericIsoCode = 50,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Belarus",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "BY",
                                        ThreeLetterIsoCode = "BLR",
                                        NumericIsoCode = 112,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Belgium",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "BE",
                                        ThreeLetterIsoCode = "BEL",
                                        NumericIsoCode = 56,
                                        SubjectToVat = true,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Belize",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "BZ",
                                        ThreeLetterIsoCode = "BLZ",
                                        NumericIsoCode = 84,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Bermuda",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "BM",
                                        ThreeLetterIsoCode = "BMU",
                                        NumericIsoCode = 60,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Bolivia",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "BO",
                                        ThreeLetterIsoCode = "BOL",
                                        NumericIsoCode = 68,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Bosnia and Herzegowina",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "BA",
                                        ThreeLetterIsoCode = "BIH",
                                        NumericIsoCode = 70,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Brazil",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "BR",
                                        ThreeLetterIsoCode = "BRA",
                                        NumericIsoCode = 76,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Bulgaria",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "BG",
                                        ThreeLetterIsoCode = "BGR",
                                        NumericIsoCode = 100,
                                        SubjectToVat = true,
                                        DisplayOrder = 100,
                                        Published = true
                                    },
                                    new Country
                                    {
                                        Name = "Cayman Islands",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "KY",
                                        ThreeLetterIsoCode = "CYM",
                                        NumericIsoCode = 136,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Chile",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "CL",
                                        ThreeLetterIsoCode = "CHL",
                                        NumericIsoCode = 152,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "China",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "CN",
                                        ThreeLetterIsoCode = "CHN",
                                        NumericIsoCode = 156,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Colombia",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "CO",
                                        ThreeLetterIsoCode = "COL",
                                        NumericIsoCode = 170,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Costa Rica",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "CR",
                                        ThreeLetterIsoCode = "CRI",
                                        NumericIsoCode = 188,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Croatia",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "HR",
                                        ThreeLetterIsoCode = "HRV",
                                        NumericIsoCode = 191,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Cuba",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "CU",
                                        ThreeLetterIsoCode = "CUB",
                                        NumericIsoCode = 192,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Cyprus",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "CY",
                                        ThreeLetterIsoCode = "CYP",
                                        NumericIsoCode = 196,
                                        SubjectToVat = true,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Czech Republic",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "CZ",
                                        ThreeLetterIsoCode = "CZE",
                                        NumericIsoCode = 203,
                                        SubjectToVat = true,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Denmark",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "DK",
                                        ThreeLetterIsoCode = "DNK",
                                        NumericIsoCode = 208,
                                        SubjectToVat = true,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Dominican Republic",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "DO",
                                        ThreeLetterIsoCode = "DOM",
                                        NumericIsoCode = 214,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "East Timor",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "TL",
                                        ThreeLetterIsoCode = "TLS",
                                        NumericIsoCode = 626,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Ecuador",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "EC",
                                        ThreeLetterIsoCode = "ECU",
                                        NumericIsoCode = 218,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Egypt",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "EG",
                                        ThreeLetterIsoCode = "EGY",
                                        NumericIsoCode = 818,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Finland",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "FI",
                                        ThreeLetterIsoCode = "FIN",
                                        NumericIsoCode = 246,
                                        SubjectToVat = true,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "France",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "FR",
                                        ThreeLetterIsoCode = "FRA",
                                        NumericIsoCode = 250,
                                        SubjectToVat = true,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Georgia",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "GE",
                                        ThreeLetterIsoCode = "GEO",
                                        NumericIsoCode = 268,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Germany",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "DE",
                                        ThreeLetterIsoCode = "DEU",
                                        NumericIsoCode = 276,
                                        SubjectToVat = true,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Gibraltar",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "GI",
                                        ThreeLetterIsoCode = "GIB",
                                        NumericIsoCode = 292,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Greece",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "GR",
                                        ThreeLetterIsoCode = "GRC",
                                        NumericIsoCode = 300,
                                        SubjectToVat = true,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Guatemala",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "GT",
                                        ThreeLetterIsoCode = "GTM",
                                        NumericIsoCode = 320,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Hong Kong",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "HK",
                                        ThreeLetterIsoCode = "HKG",
                                        NumericIsoCode = 344,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Hungary",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "HU",
                                        ThreeLetterIsoCode = "HUN",
                                        NumericIsoCode = 348,
                                        SubjectToVat = true,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "India",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "IN",
                                        ThreeLetterIsoCode = "IND",
                                        NumericIsoCode = 356,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Indonesia",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "ID",
                                        ThreeLetterIsoCode = "IDN",
                                        NumericIsoCode = 360,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Ireland",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "IE",
                                        ThreeLetterIsoCode = "IRL",
                                        NumericIsoCode = 372,
                                        SubjectToVat = true,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Israel",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "IL",
                                        ThreeLetterIsoCode = "ISR",
                                        NumericIsoCode = 376,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Italy",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "IT",
                                        ThreeLetterIsoCode = "ITA",
                                        NumericIsoCode = 380,
                                        SubjectToVat = true,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Jamaica",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "JM",
                                        ThreeLetterIsoCode = "JAM",
                                        NumericIsoCode = 388,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Japan",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "JP",
                                        ThreeLetterIsoCode = "JPN",
                                        NumericIsoCode = 392,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Jordan",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "JO",
                                        ThreeLetterIsoCode = "JOR",
                                        NumericIsoCode = 400,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Kazakhstan",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "KZ",
                                        ThreeLetterIsoCode = "KAZ",
                                        NumericIsoCode = 398,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Korea, Democratic People's Republic of",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "KP",
                                        ThreeLetterIsoCode = "PRK",
                                        NumericIsoCode = 408,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Kuwait",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "KW",
                                        ThreeLetterIsoCode = "KWT",
                                        NumericIsoCode = 414,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Malaysia",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "MY",
                                        ThreeLetterIsoCode = "MYS",
                                        NumericIsoCode = 458,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Mexico",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "MX",
                                        ThreeLetterIsoCode = "MEX",
                                        NumericIsoCode = 484,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Netherlands",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "NL",
                                        ThreeLetterIsoCode = "NLD",
                                        NumericIsoCode = 528,
                                        SubjectToVat = true,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "New Zealand",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "NZ",
                                        ThreeLetterIsoCode = "NZL",
                                        NumericIsoCode = 554,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Norway",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "NO",
                                        ThreeLetterIsoCode = "NOR",
                                        NumericIsoCode = 578,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Pakistan",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "PK",
                                        ThreeLetterIsoCode = "PAK",
                                        NumericIsoCode = 586,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Palestine",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "PS",
                                        ThreeLetterIsoCode = "PSE",
                                        NumericIsoCode = 275,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Paraguay",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "PY",
                                        ThreeLetterIsoCode = "PRY",
                                        NumericIsoCode = 600,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Peru",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "PE",
                                        ThreeLetterIsoCode = "PER",
                                        NumericIsoCode = 604,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Philippines",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "PH",
                                        ThreeLetterIsoCode = "PHL",
                                        NumericIsoCode = 608,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Poland",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "PL",
                                        ThreeLetterIsoCode = "POL",
                                        NumericIsoCode = 616,
                                        SubjectToVat = true,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Portugal",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "PT",
                                        ThreeLetterIsoCode = "PRT",
                                        NumericIsoCode = 620,
                                        SubjectToVat = true,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Puerto Rico",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "PR",
                                        ThreeLetterIsoCode = "PRI",
                                        NumericIsoCode = 630,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Qatar",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "QA",
                                        ThreeLetterIsoCode = "QAT",
                                        NumericIsoCode = 634,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Romania",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "RO",
                                        ThreeLetterIsoCode = "ROM",
                                        NumericIsoCode = 642,
                                        SubjectToVat = true,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Russian Federation",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "RU",
                                        ThreeLetterIsoCode = "RUS",
                                        NumericIsoCode = 643,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Saudi Arabia",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "SA",
                                        ThreeLetterIsoCode = "SAU",
                                        NumericIsoCode = 682,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Singapore",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "SG",
                                        ThreeLetterIsoCode = "SGP",
                                        NumericIsoCode = 702,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Slovakia (Slovak Republic)",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "SK",
                                        ThreeLetterIsoCode = "SVK",
                                        NumericIsoCode = 703,
                                        SubjectToVat = true,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Slovenia",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "SI",
                                        ThreeLetterIsoCode = "SVN",
                                        NumericIsoCode = 705,
                                        SubjectToVat = true,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "South Africa",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "ZA",
                                        ThreeLetterIsoCode = "ZAF",
                                        NumericIsoCode = 710,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Spain",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "ES",
                                        ThreeLetterIsoCode = "ESP",
                                        NumericIsoCode = 724,
                                        SubjectToVat = true,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Sweden",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "SE",
                                        ThreeLetterIsoCode = "SWE",
                                        NumericIsoCode = 752,
                                        SubjectToVat = true,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Switzerland",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "CH",
                                        ThreeLetterIsoCode = "CHE",
                                        NumericIsoCode = 756,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Taiwan",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "TW",
                                        ThreeLetterIsoCode = "TWN",
                                        NumericIsoCode = 158,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Thailand",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "TH",
                                        ThreeLetterIsoCode = "THA",
                                        NumericIsoCode = 764,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Turkey",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "TR",
                                        ThreeLetterIsoCode = "TUR",
                                        NumericIsoCode = 792,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Ukraine",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "UA",
                                        ThreeLetterIsoCode = "UKR",
                                        NumericIsoCode = 804,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "United Arab Emirates",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "AE",
                                        ThreeLetterIsoCode = "ARE",
                                        NumericIsoCode = 784,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "United Kingdom",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "GB",
                                        ThreeLetterIsoCode = "GBR",
                                        NumericIsoCode = 826,
                                        SubjectToVat = true,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "United States minor outlying islands",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "UM",
                                        ThreeLetterIsoCode = "UMI",
                                        NumericIsoCode = 581,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Uruguay",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "UY",
                                        ThreeLetterIsoCode = "URY",
                                        NumericIsoCode = 858,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Uzbekistan",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "UZ",
                                        ThreeLetterIsoCode = "UZB",
                                        NumericIsoCode = 860,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Venezuela",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "VE",
                                        ThreeLetterIsoCode = "VEN",
                                        NumericIsoCode = 862,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Serbia",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "RS",
                                        ThreeLetterIsoCode = "SRB",
                                        NumericIsoCode = 688,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Afghanistan",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "AF",
                                        ThreeLetterIsoCode = "AFG",
                                        NumericIsoCode = 4,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Albania",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "AL",
                                        ThreeLetterIsoCode = "ALB",
                                        NumericIsoCode = 8,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Algeria",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "DZ",
                                        ThreeLetterIsoCode = "DZA",
                                        NumericIsoCode = 12,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "American Samoa",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "AS",
                                        ThreeLetterIsoCode = "ASM",
                                        NumericIsoCode = 16,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Andorra",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "AD",
                                        ThreeLetterIsoCode = "AND",
                                        NumericIsoCode = 20,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Angola",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "AO",
                                        ThreeLetterIsoCode = "AGO",
                                        NumericIsoCode = 24,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Anguilla",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "AI",
                                        ThreeLetterIsoCode = "AIA",
                                        NumericIsoCode = 660,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Antarctica",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "AQ",
                                        ThreeLetterIsoCode = "ATA",
                                        NumericIsoCode = 10,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Antigua and Barbuda",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "AG",
                                        ThreeLetterIsoCode = "ATG",
                                        NumericIsoCode = 28,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Bahrain",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "BH",
                                        ThreeLetterIsoCode = "BHR",
                                        NumericIsoCode = 48,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Barbados",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "BB",
                                        ThreeLetterIsoCode = "BRB",
                                        NumericIsoCode = 52,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    },
                                    new Country
                                    {
                                        Name = "Benin",
                                        AllowsBilling = true,
                                        AllowsShipping = true,
                                        TwoLetterIsoCode = "BJ",
                                        ThreeLetterIsoCode = "BEN",
                                        NumericIsoCode = 204,
                                        SubjectToVat = false,
                                        DisplayOrder = 100,
                                        Published = false
                                    }
                                };
            _countryRepository.Insert(countries);
        }

        protected virtual void InstallUsersAndUsers(string defaultUserEmail, string defaultUserPassword)
        {
            var crAdministrators = new UserRole
            {
                Name = "Administrators",
                Active = true,
                IsSystemRole = true,
                SystemName = SystemUserRoleNames.Administrators,
            };
            var crGeneralManager = new UserRole
            {
                Name = "General Manager",
                Active = true,
                IsSystemRole = true,
                SystemName = SystemUserRoleNames.GeneralManager,
            };
            var crRegionalManager = new UserRole
            {
                Name = "Regional Manager",
                Active = true,
                IsSystemRole = true,
                SystemName = SystemUserRoleNames.RegionalManager,
            };
            var crSupervisor = new UserRole
            {
                Name = "Supervisor",
                Active = true,
                IsSystemRole = true,
                SystemName = SystemUserRoleNames.Supervisor,
            };
            var crTeamLeader = new UserRole
            {
                Name = "Team Leader",
                Active = true,
                IsSystemRole = true,
                SystemName = SystemUserRoleNames.TeamLeader,
            };
            var crOperator = new UserRole
            {
                Name = "Operator",
                Active = true,
                IsSystemRole = true,
                SystemName = SystemUserRoleNames.Operator,
            };
            var crRegistered = new UserRole
            {
                Name = "Registered",
                Active = true,
                IsSystemRole = true,
                SystemName = SystemUserRoleNames.Registered,
            };
            var crGuests = new UserRole
            {
                Name = "Guests",
                Active = true,
                IsSystemRole = true,
                SystemName = SystemUserRoleNames.Guests,
            };

            var UserRoles = new List<UserRole>
                                {
                                    crAdministrators,
                                    crRegistered,
                                    crGuests,
                                    crGeneralManager,
                                    crRegionalManager,
                                    crSupervisor,
                                    crTeamLeader,
                                    crOperator,
                                };
            _UserRoleRepository.Insert(UserRoles);

            //default store 
            var defaultStore = _storeRepository.Table.FirstOrDefault();

            if (defaultStore == null)
                throw new Exception("No default store could be loaded");

            var storeId = defaultStore.Id;

            //admin user
            var adminUser = new User
            {
                UserGuid = Guid.NewGuid(),
                Email = defaultUserEmail,
                Username = defaultUserEmail,
                Active = true,
                CreatedOnUtc = DateTime.UtcNow,
                LastActivityDateUtc = DateTime.UtcNow,
                RegisteredInStoreId = storeId
            };

            var defaultAdminUserAddress = new Address
            {
                FirstName = "John",
                LastName = "Smith",
                PhoneNumber = "12345678",
                Email = defaultUserEmail,
                FaxNumber = "",
                Company = "Nop Solutions Ltd",
                Address1 = "21 West 52nd Street",
                Address2 = "",
                City = "New York",
                StateProvince = _stateProvinceRepository.Table.FirstOrDefault(sp => sp.Name == "New York"),
                Country = _countryRepository.Table.FirstOrDefault(c => c.ThreeLetterIsoCode == "USA"),
                ZipPostalCode = "10021",
                CreatedOnUtc = DateTime.UtcNow,
            };
            adminUser.Addresses.Add(defaultAdminUserAddress);
            adminUser.BillingAddress = defaultAdminUserAddress;
            adminUser.ShippingAddress = defaultAdminUserAddress;

            adminUser.UserRoles.Add(crAdministrators);
            adminUser.UserRoles.Add(crRegistered);

            _UserRepository.Insert(adminUser);
            //set default User name
            _genericAttributeService.SaveAttribute(adminUser, SystemUserAttributeNames.FirstName, "John");
            _genericAttributeService.SaveAttribute(adminUser, SystemUserAttributeNames.LastName, "Smith");

            //set hashed admin password
            var UserRegistrationService = EngineContext.Current.Resolve<IUserRegistrationService>();
            UserRegistrationService.ChangePassword(new ChangePasswordRequest(defaultUserEmail, false,
                 PasswordFormat.Hashed, defaultUserPassword));           
        }
        
        protected virtual void InstallSettings(bool installSampleData)
        {
            var settingService = EngineContext.Current.Resolve<ISettingService>();
            settingService.SaveSetting(new PdfSettings
            {
                LogoPictureId = 0,
                LetterPageSizeEnabled = false,
                RenderOrderNotes = true,
                FontFileName = "FreeSerif.ttf",
                InvoiceFooterTextColumn1 = null,
                InvoiceFooterTextColumn2 = null,
            });

            settingService.SaveSetting(new CommonSettings
            {
                UseSystemEmailForContactUsForm = true,
                UseStoredProceduresIfSupported = true,
                UseStoredProcedureForLoadingCategories = false,
                SitemapEnabled = true,
                SitemapIncludeCategories = true,
                SitemapIncludeManufacturers = true,
                SitemapIncludeProducts = false,
                DisplayJavaScriptDisabledWarning = false,
                UseFullTextSearch = false,
                FullTextMode = FulltextSearchMode.ExactMatch,
                Log404Errors = true,
                BreadcrumbDelimiter = "/",
                RenderXuaCompatible = false,
                XuaCompatibleValue = "IE=edge",
                BbcodeEditorOpenLinksInNewWindow = false
            });
            
            settingService.SaveSetting(new AdminAreaSettings
            {
                DefaultGridPageSize = 15,
                PopupGridPageSize = 10,
                GridPageSizes = "10, 15, 20, 50, 100",
                RichEditorAdditionalSettings = null,
                RichEditorAllowJavaScript = false,
                UseRichEditorInMessageTemplates = false,
                UseIsoDateTimeConverterInJson = true
            });

            settingService.SaveSetting(new LocalizationSettings
            {
                DefaultAdminLanguageId = _languageRepository.Table.Single(l => l.Name == "English").Id,
                UseImagesForLanguageSelection = false,
                SeoFriendlyUrlsForLanguagesEnabled = false,
                AutomaticallyDetectLanguage = false,
                LoadAllLocaleRecordsOnStartup = true,
                LoadAllLocalizedPropertiesOnStartup = true,
                LoadAllUrlRecordsOnStartup = false,
                IgnoreRtlPropertyForAdminArea = false
            });

            settingService.SaveSetting(new UserSettings
            {
                UsernamesEnabled = true,
                CheckUsernameAvailabilityEnabled = false,
                AllowUsersToChangeUsernames = false,
                DefaultPasswordFormat = PasswordFormat.Hashed,
                HashedPasswordFormat = "SHA1",
                PasswordMinLength = 6,
                UnduplicatedPasswordsNumber = 4,
                PasswordRecoveryLinkDaysValid = 7,
                PasswordLifetime = 90,
                FailedPasswordAllowedAttempts = 0,
                FailedPasswordLockoutMinutes = 30,
                UserRegistrationType = UserRegistrationType.Standard,
                AllowUsersToUploadAvatars = false,
                AvatarMaximumSizeBytes = 20000,
                DefaultAvatarEnabled = true,
                ShowUsersLocation = false,
                ShowUsersJoinDate = false,
                AllowViewingProfiles = false,
                NotifyNewUserRegistration = false,
                HideDownloadableProductsTab = false,
                HideBackInStockSubscriptionsTab = false,
                DownloadableProductsValidateUser = false,
                UserNameFormat = UserNameFormat.ShowFirstName,
                GenderEnabled = true,
                DateOfBirthEnabled = true,
                DateOfBirthRequired = false,
                DateOfBirthMinimumAge = null,
                CompanyEnabled = true,
                StreetAddressEnabled = false,
                StreetAddress2Enabled = false,
                ZipPostalCodeEnabled = false,
                CityEnabled = false,
                CountryEnabled = false,
                CountryRequired = false,
                StateProvinceEnabled = false,
                StateProvinceRequired = false,
                PhoneEnabled = false,
                FaxEnabled = false,
                AcceptPrivacyPolicyEnabled = false,
                NewsletterEnabled = true,
                NewsletterTickedByDefault = true,
                HideNewsletterBlock = false,
                NewsletterBlockAllowToUnsubscribe = false,
                OnlineUserMinutes = 20,
                StoreLastVisitedPage = false,
                SuffixDeletedUsers = false,
                EnteringEmailTwice = false,
                RequireRegistrationForDownloadableProducts = false,
                DeleteGuestTaskOlderThanMinutes = 1440
            });

            settingService.SaveSetting(new AddressSettings
            {
                CompanyEnabled = true,
                StreetAddressEnabled = true,
                StreetAddressRequired = true,
                StreetAddress2Enabled = true,
                ZipPostalCodeEnabled = true,
                ZipPostalCodeRequired = true,
                CityEnabled = true,
                CityRequired = true,
                CountryEnabled = true,
                StateProvinceEnabled = true,
                PhoneEnabled = true,
                PhoneRequired = true,
                FaxEnabled = true,
            });

            settingService.SaveSetting(new MediaSettings
            {
                AvatarPictureSize = 120,
                ProductThumbPictureSize = 415,
                ProductDetailsPictureSize = 550,
                ProductThumbPictureSizeOnProductDetailsPage = 100,
                AssociatedProductPictureSize = 220,
                CategoryThumbPictureSize = 450,
                ManufacturerThumbPictureSize = 420,
                VendorThumbPictureSize = 450,
                CartThumbPictureSize = 80,
                MiniCartThumbPictureSize = 70,
                AutoCompleteSearchThumbPictureSize = 20,
                ImageSquarePictureSize = 32,
                MaximumImageSize = 1980,
                DefaultPictureZoomEnabled = false,
                DefaultImageQuality = 80,
                MultipleThumbDirectories = false,
                ImportProductImagesUsingHash = true,
                AzureCacheControlHeader = string.Empty
            });

            settingService.SaveSetting(new ExternalAuthenticationSettings
            {
                AutoRegisterEnabled = true,
                RequireEmailValidation = false
            });

            settingService.SaveSetting(new CurrencySettings
            {
                DisplayCurrencyLabel = false,
                PrimaryStoreCurrencyId = _currencyRepository.Table.Single(c => c.CurrencyCode == "USD").Id,
                PrimaryExchangeRateCurrencyId = _currencyRepository.Table.Single(c => c.CurrencyCode == "USD").Id,
                ActiveExchangeRateProviderSystemName = "CurrencyExchange.MoneyConverter",
                AutoUpdateEnabled = false
            });

            settingService.SaveSetting(new MeasureSettings
            {
                BaseDimensionId = _measureDimensionRepository.Table.Single(m => m.SystemKeyword == "inches").Id,
                BaseWeightId = _measureWeightRepository.Table.Single(m => m.SystemKeyword == "lb").Id,
            });

            settingService.SaveSetting(new MessageTemplatesSettings
            {
                CaseInvariantReplacement = false,
                Color1 = "#b9babe",
                Color2 = "#ebecee",
                Color3 = "#dde2e6",
            });

            settingService.SaveSetting(new SecuritySettings
            {
                ForceSslForAllPages = false,
                EncryptionKey = CommonHelper.GenerateRandomDigitCode(16),
                AdminAreaAllowedIpAddresses = null,
                EnableXsrfProtectionForAdminArea = true,
                EnableXsrfProtectionForPublicStore = true,
                HoneypotEnabled = false,
                HoneypotInputName = "hpinput"
            });
            
            settingService.SaveSetting(new DateTimeSettings
            {
                DefaultStoreTimeZoneId = "",
                AllowUsersToSetTimeZone = false
            });

            var eaGeneral = _emailAccountRepository.Table.FirstOrDefault();
            if (eaGeneral == null)
                throw new Exception("Default email account cannot be loaded");
            settingService.SaveSetting(new EmailAccountSettings
            {
                DefaultEmailAccountId = eaGeneral.Id
            });

            settingService.SaveSetting(new DisplayDefaultMenuItemSettings
            {
                DisplayHomePageMenuItem = !installSampleData,
                DisplayNewProductsMenuItem = !installSampleData,
                DisplayProductSearchMenuItem = !installSampleData,
                DisplayUserInfoMenuItem = !installSampleData,
                DisplayBlogMenuItem = !installSampleData,
                DisplayForumsMenuItem = !installSampleData,
                DisplayContactUsMenuItem = !installSampleData
            });
        }

        protected virtual void InstallActivityLogTypes()
        {
            var activityLogTypes = new List<ActivityLogType>
            {
                //admin area activities
                new ActivityLogType
                {
                    SystemKeyword = "AddNewAddressAttribute",
                    Enabled = true,
                    Name = "Add a new address attribute"
                },
                new ActivityLogType
                {
                    SystemKeyword = "AddNewAddressAttributeValue",
                    Enabled = true,
                    Name = "Add a new address attribute value"
                },
                //new ActivityLogType
                //{
                //    SystemKeyword = "AddNewAffiliate",
                //    Enabled = true,
                //    Name = "Add a new affiliate"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "AddNewBlogPost",
                //    Enabled = true,
                //    Name = "Add a new blog post"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "AddNewCampaign",
                //    Enabled = true,
                //    Name = "Add a new campaign"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "AddNewCategory",
                //    Enabled = true,
                //    Name = "Add a new category"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "AddNewCheckoutAttribute",
                //    Enabled = true,
                //    Name = "Add a new checkout attribute"
                //},
                new ActivityLogType
                {
                    SystemKeyword = "AddNewCountry",
                    Enabled = true,
                    Name = "Add a new country"
                },
                new ActivityLogType
                {
                    SystemKeyword = "AddNewCurrency",
                    Enabled = true,
                    Name = "Add a new currency"
                },
                new ActivityLogType
                {
                    SystemKeyword = "AddNewUser",
                    Enabled = true,
                    Name = "Add a new User"
                },
                new ActivityLogType
                {
                    SystemKeyword = "AddNewUserAttribute",
                    Enabled = true,
                    Name = "Add a new User attribute"
                },
                new ActivityLogType
                {
                    SystemKeyword = "AddNewUserAttributeValue",
                    Enabled = true,
                    Name = "Add a new User attribute value"
                },
                new ActivityLogType
                {
                    SystemKeyword = "AddNewUserRole",
                    Enabled = true,
                    Name = "Add a new User role"
                },
                //new ActivityLogType
                //{
                //    SystemKeyword = "AddNewDiscount",
                //    Enabled = true,
                //    Name = "Add a new discount"
                //},
                new ActivityLogType
                {
                    SystemKeyword = "AddNewEmailAccount",
                    Enabled = true,
                    Name = "Add a new email account"
                },
                //new ActivityLogType
                //{
                //    SystemKeyword = "AddNewGiftCard",
                //    Enabled = true,
                //    Name = "Add a new gift card"
                //},
                new ActivityLogType
                {
                    SystemKeyword = "AddNewLanguage",
                    Enabled = true,
                    Name = "Add a new language"
                },
                new ActivityLogType
                {
                    SystemKeyword = "AddNewManufacturer",
                    Enabled = true,
                    Name = "Add a new manufacturer"
                },
                new ActivityLogType
                {
                    SystemKeyword = "AddNewMeasureDimension",
                    Enabled = true,
                    Name = "Add a new measure dimension"
                },
                new ActivityLogType
                {
                    SystemKeyword = "AddNewMeasureWeight",
                    Enabled = true,
                    Name = "Add a new measure weight"
                },
                //new ActivityLogType
                //{
                //    SystemKeyword = "AddNewNews",
                //    Enabled = true,
                //    Name = "Add a new news"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "AddNewProduct",
                //    Enabled = true,
                //    Name = "Add a new product"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "AddNewProductAttribute",
                //    Enabled = true,
                //    Name = "Add a new product attribute"
                //},
                new ActivityLogType
                {
                    SystemKeyword = "AddNewSetting",
                    Enabled = true,
                    Name = "Add a new setting"
                },
                //new ActivityLogType
                //{
                //    SystemKeyword = "AddNewSpecAttribute",
                //    Enabled = true,
                //    Name = "Add a new specification attribute"
                //},
                new ActivityLogType
                {
                    SystemKeyword = "AddNewStateProvince",
                    Enabled = true,
                    Name = "Add a new state or province"
                },
                //new ActivityLogType
                //{
                //    SystemKeyword = "AddNewStore",
                //    Enabled = true,
                //    Name = "Add a new store"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "AddNewTopic",
                //    Enabled = true,
                //    Name = "Add a new topic"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "AddNewVendor",
                //    Enabled = true,
                //    Name = "Add a new vendor"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "AddNewWarehouse",
                //    Enabled = true,
                //    Name = "Add a new warehouse"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "AddNewWidget",
                //    Enabled = true,
                //    Name = "Add a new widget"
                //},
                new ActivityLogType
                {
                    SystemKeyword = "DeleteActivityLog",
                    Enabled = true,
                    Name = "Delete activity log"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteAddressAttribute",
                    Enabled = true,
                    Name = "Delete an address attribute"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteAddressAttributeValue",
                    Enabled = true,
                    Name = "Delete an address attribute value"
                },
                //new ActivityLogType
                //{
                //    SystemKeyword = "DeleteAffiliate",
                //    Enabled = true,
                //    Name = "Delete an affiliate"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "DeleteBlogPost",
                //    Enabled = true,
                //    Name = "Delete a blog post"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "DeleteBlogPostComment",
                //    Enabled = true,
                //    Name = "Delete a blog post comment"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "DeleteCampaign",
                //    Enabled = true,
                //    Name = "Delete a campaign"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "DeleteCategory",
                //    Enabled = true,
                //    Name = "Delete category"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "DeleteCheckoutAttribute",
                //    Enabled = true,
                //    Name = "Delete a checkout attribute"
                //},
                new ActivityLogType
                {
                    SystemKeyword = "DeleteCountry",
                    Enabled = true,
                    Name = "Delete a country"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteCurrency",
                    Enabled = true,
                    Name = "Delete a currency"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteUser",
                    Enabled = true,
                    Name = "Delete a User"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteUserAttribute",
                    Enabled = true,
                    Name = "Delete a User attribute"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteUserAttributeValue",
                    Enabled = true,
                    Name = "Delete a User attribute value"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteUserRole",
                    Enabled = true,
                    Name = "Delete a User role"
                },
                //new ActivityLogType
                //{
                //    SystemKeyword = "DeleteDiscount",
                //    Enabled = true,
                //    Name = "Delete a discount"
                //},
                new ActivityLogType
                {
                    SystemKeyword = "DeleteEmailAccount",
                    Enabled = true,
                    Name = "Delete an email account"
                },
                //new ActivityLogType
                //{
                //    SystemKeyword = "DeleteGiftCard",
                //    Enabled = true,
                //    Name = "Delete a gift card"
                //},
                new ActivityLogType
                {
                    SystemKeyword = "DeleteLanguage",
                    Enabled = true,
                    Name = "Delete a language"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteManufacturer",
                    Enabled = true,
                    Name = "Delete a manufacturer"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteMeasureDimension",
                    Enabled = true,
                    Name = "Delete a measure dimension"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteMeasureWeight",
                    Enabled = true,
                    Name = "Delete a measure weight"
                },
                //new ActivityLogType
                //{
                //    SystemKeyword = "DeleteMessageTemplate",
                //    Enabled = true,
                //    Name = "Delete a message template"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "DeleteNews",
                //    Enabled = true,
                //    Name = "Delete a news"
                //},
                // new ActivityLogType
                //{
                //    SystemKeyword = "DeleteNewsComment",
                //    Enabled = true,
                //    Name = "Delete a news comment"
                //},
                new ActivityLogType
                {
                    SystemKeyword = "DeleteOrder",
                    Enabled = true,
                    Name = "Delete an order"
                },
                //new ActivityLogType
                //{
                //    SystemKeyword = "DeleteProduct",
                //    Enabled = true,
                //    Name = "Delete a product"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "DeleteProductAttribute",
                //    Enabled = true,
                //    Name = "Delete a product attribute"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "DeleteProductReview",
                //    Enabled = true,
                //    Name = "Delete a product review"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "DeleteReturnRequest",
                //    Enabled = true,
                //    Name = "Delete a return request"
                //},
                new ActivityLogType
                {
                    SystemKeyword = "DeleteSetting",
                    Enabled = true,
                    Name = "Delete a setting"
                },
                //new ActivityLogType
                //{
                //    SystemKeyword = "DeleteSpecAttribute",
                //    Enabled = true,
                //    Name = "Delete a specification attribute"
                //},
                new ActivityLogType
                {
                    SystemKeyword = "DeleteStateProvince",
                    Enabled = true,
                    Name = "Delete a state or province"
                },
                //new ActivityLogType
                //{
                //    SystemKeyword = "DeleteStore",
                //    Enabled = true,
                //    Name = "Delete a store"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "DeleteTopic",
                //    Enabled = true,
                //    Name = "Delete a topic"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "DeleteVendor",
                //    Enabled = true,
                //    Name = "Delete a vendor"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "DeleteWarehouse",
                //    Enabled = true,
                //    Name = "Delete a warehouse"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "DeleteWidget",
                //    Enabled = true,
                //    Name = "Delete a widget"
                //},
                new ActivityLogType
                {
                    SystemKeyword = "EditActivityLogTypes",
                    Enabled = true,
                    Name = "Edit activity log types"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditAddressAttribute",
                    Enabled = true,
                    Name = "Edit an address attribute"
                },
                 new ActivityLogType
                {
                    SystemKeyword = "EditAddressAttributeValue",
                    Enabled = true,
                    Name = "Edit an address attribute value"
                },
                //new ActivityLogType
                //{
                //    SystemKeyword = "EditAffiliate",
                //    Enabled = true,
                //    Name = "Edit an affiliate"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "EditBlogPost",
                //    Enabled = true,
                //    Name = "Edit a blog post"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "EditCampaign",
                //    Enabled = true,
                //    Name = "Edit a campaign"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "EditCategory",
                //    Enabled = true,
                //    Name = "Edit category"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "EditCheckoutAttribute",
                //    Enabled = true,
                //    Name = "Edit a checkout attribute"
                //},
                new ActivityLogType
                {
                    SystemKeyword = "EditCountry",
                    Enabled = true,
                    Name = "Edit a country"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditCurrency",
                    Enabled = true,
                    Name = "Edit a currency"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditUser",
                    Enabled = true,
                    Name = "Edit a User"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditUserAttribute",
                    Enabled = true,
                    Name = "Edit a User attribute"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditUserAttributeValue",
                    Enabled = true,
                    Name = "Edit a User attribute value"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditUserRole",
                    Enabled = true,
                    Name = "Edit a User role"
                },
                //new ActivityLogType
                //{
                //    SystemKeyword = "EditDiscount",
                //    Enabled = true,
                //    Name = "Edit a discount"
                //},
                new ActivityLogType
                {
                    SystemKeyword = "EditEmailAccount",
                    Enabled = true,
                    Name = "Edit an email account"
                },
                //new ActivityLogType
                //{
                //    SystemKeyword = "EditGiftCard",
                //    Enabled = true,
                //    Name = "Edit a gift card"
                //},
                new ActivityLogType
                {
                    SystemKeyword = "EditLanguage",
                    Enabled = true,
                    Name = "Edit a language"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditManufacturer",
                    Enabled = true,
                    Name = "Edit a manufacturer"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditMeasureDimension",
                    Enabled = true,
                    Name = "Edit a measure dimension"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditMeasureWeight",
                    Enabled = true,
                    Name = "Edit a measure weight"
                },
                //new ActivityLogType
                //{
                //    SystemKeyword = "EditMessageTemplate",
                //    Enabled = true,
                //    Name = "Edit a message template"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "EditNews",
                //    Enabled = true,
                //    Name = "Edit a news"
                //},
                new ActivityLogType
                {
                    SystemKeyword = "EditOrder",
                    Enabled = true,
                    Name = "Edit an order"
                },
                //new ActivityLogType
                //{
                //    SystemKeyword = "EditPlugin",
                //    Enabled = true,
                //    Name = "Edit a plugin"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "EditProduct",
                //    Enabled = true,
                //    Name = "Edit a product"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "EditProductAttribute",
                //    Enabled = true,
                //    Name = "Edit a product attribute"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "EditProductReview",
                //    Enabled = true,
                //    Name = "Edit a product review"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "EditPromotionProviders",
                //    Enabled = true,
                //    Name = "Edit promotion providers"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "EditReturnRequest",
                //    Enabled = true,
                //    Name = "Edit a return request"
                //},
                new ActivityLogType
                {
                    SystemKeyword = "EditSettings",
                    Enabled = true,
                    Name = "Edit setting(s)"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditStateProvince",
                    Enabled = true,
                    Name = "Edit a state or province"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditStore",
                    Enabled = true,
                    Name = "Edit a store"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditTask",
                    Enabled = true,
                    Name = "Edit a task"
                },
                //new ActivityLogType
                //{
                //    SystemKeyword = "EditSpecAttribute",
                //    Enabled = true,
                //    Name = "Edit a specification attribute"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "EditVendor",
                //    Enabled = true,
                //    Name = "Edit a vendor"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "EditWarehouse",
                //    Enabled = true,
                //    Name = "Edit a warehouse"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "EditTopic",
                //    Enabled = true,
                //    Name = "Edit a topic"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "EditWidget",
                //    Enabled = true,
                //    Name = "Edit a widget"
                //},
                new ActivityLogType
                {
                    SystemKeyword = "Impersonation.Started",
                    Enabled = true,
                    Name = "User impersonation session. Started"
                },
                new ActivityLogType
                {
                    SystemKeyword = "Impersonation.Finished",
                    Enabled = true,
                    Name = "User impersonation session. Finished"
                },
                //new ActivityLogType
                //{
                //    SystemKeyword = "ImportCategories",
                //    Enabled = true,
                //    Name = "Categories were imported"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "ImportManufacturers",
                //    Enabled = true,
                //    Name = "Manufacturers were imported"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "ImportProducts",
                //    Enabled = true,
                //    Name = "Products were imported"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "ImportStates",
                //    Enabled = true,
                //    Name = "States were imported"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "InstallNewPlugin",
                //    Enabled = true,
                //    Name = "Install a new plugin"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "UninstallPlugin",
                //    Enabled = true,
                //    Name = "Uninstall a plugin"
                //},
                //public store activities
                //new ActivityLogType
                //{
                //    SystemKeyword = "PublicStore.ViewCategory",
                //    Enabled = false,
                //    Name = "Public store. View a category"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "PublicStore.ViewManufacturer",
                //    Enabled = false,
                //    Name = "Public store. View a manufacturer"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "PublicStore.ViewProduct",
                //    Enabled = false,
                //    Name = "Public store. View a product"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "PublicStore.PlaceOrder",
                //    Enabled = false,
                //    Name = "Public store. Place an order"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "PublicStore.SendPM",
                //    Enabled = false,
                //    Name = "Public store. Send PM"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "PublicStore.ContactUs",
                //    Enabled = false,
                //    Name = "Public store. Use contact us form"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "PublicStore.AddToCompareList",
                //    Enabled = false,
                //    Name = "Public store. Add to compare list"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "PublicStore.AddToShoppingCart",
                //    Enabled = false,
                //    Name = "Public store. Add to shopping cart"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "PublicStore.AddToWishlist",
                //    Enabled = false,
                //    Name = "Public store. Add to wishlist"
                //},
                new ActivityLogType
                {
                    SystemKeyword = "PublicStore.Login",
                    Enabled = false,
                    Name = "Public store. Login"
                },
                new ActivityLogType
                {
                    SystemKeyword = "PublicStore.Logout",
                    Enabled = false,
                    Name = "Public store. Logout"
                },
                //new ActivityLogType
                //{
                //    SystemKeyword = "PublicStore.AddProductReview",
                //    Enabled = false,
                //    Name = "Public store. Add product review"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "PublicStore.AddNewsComment",
                //    Enabled = false,
                //    Name = "Public store. Add news comment"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "PublicStore.AddBlogComment",
                //    Enabled = false,
                //    Name = "Public store. Add blog comment"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "PublicStore.AddForumTopic",
                //    Enabled = false,
                //    Name = "Public store. Add forum topic"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "PublicStore.EditForumTopic",
                //    Enabled = false,
                //    Name = "Public store. Edit forum topic"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "PublicStore.DeleteForumTopic",
                //    Enabled = false,
                //    Name = "Public store. Delete forum topic"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "PublicStore.AddForumPost",
                //    Enabled = false,
                //    Name = "Public store. Add forum post"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "PublicStore.EditForumPost",
                //    Enabled = false,
                //    Name = "Public store. Edit forum post"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = "PublicStore.DeleteForumPost",
                //    Enabled = false,
                //    Name = "Public store. Delete forum post"
                //}
            };
            _activityLogTypeRepository.Insert(activityLogTypes);
        }

        protected virtual void InstallEmailAccounts()
        {
            var emailAccounts = new List<EmailAccount>
            {
                new EmailAccount
                {
                    Email = "test@mail.com",
                    DisplayName = "Store name",
                    Host = "smtp.mail.com",
                    Port = 25,
                    Username = "123",
                    Password = "123",
                    EnableSsl = false,
                    UseDefaultCredentials = false
                },
            };
            _emailAccountRepository.Insert(emailAccounts);
        }
        protected virtual void InstallScheduleTasks()
        {
            var tasks = new List<ScheduleTask>
            {
                new ScheduleTask
                {
                    Name = "Send emails",
                    Seconds = 60,
                    Type = "Invenio.Services.Messages.QueuedMessagesSendTask, Invenio.Services",
                    Enabled = true,
                    StopOnError = false,
                },
                new ScheduleTask
                {
                    Name = "Keep alive",
                    Seconds = 300,
                    Type = "Invenio.Services.Common.KeepAliveTask, Invenio.Services",
                    Enabled = true,
                    StopOnError = false,
                },
                new ScheduleTask
                {
                    Name = "Delete guests",
                    Seconds = 600,
                    Type = "Invenio.Services.Users.DeleteGuestsTask, Invenio.Services",
                    Enabled = true,
                    StopOnError = false,
                },
                new ScheduleTask
                {
                    Name = "Clear cache",
                    Seconds = 600,
                    Type = "Invenio.Services.Caching.ClearCacheTask, Invenio.Services",
                    Enabled = false,
                    StopOnError = false,
                },
                new ScheduleTask
                {
                    Name = "Clear log",
                    //60 minutes
                    Seconds = 3600,
                    Type = "Invenio.Services.Logging.ClearLogTask, Invenio.Services",
                    Enabled = false,
                    StopOnError = false,
                },
                new ScheduleTask
                {
                    Name = "Update currency exchange rates",
                    //60 minutes
                    Seconds = 3600,
                    Type = "Invenio.Services.Directory.UpdateExchangeRateTask, Invenio.Services",
                    Enabled = true,
                    StopOnError = false,
                },
            };

            _scheduleTaskRepository.Insert(tasks);
        }
        #endregion

        #region Methods

        public virtual void InstallData(string defaultUserEmail,
            string defaultUserPassword, bool installSampleData = true)
        {
            InstallStores();
            InstallMeasures();
            InstallLanguages();
            InstallCurrencies();
            InstallCountriesAndStates();
            InstallUsersAndUsers(defaultUserEmail, defaultUserPassword);
            InstallEmailAccounts();
            InstallSettings(installSampleData);
            InstallLocaleResources();
            InstallScheduleTasks();
            InstallActivityLogTypes();
        }

        #endregion
    }
}