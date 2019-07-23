﻿using Common;
using Common.Transactions;
using Common.utilities;
using Elmah;
using Entities.enums;
using Entities.Groups;
using Entities.Repository;
using Sociatis.ActionFilters;
using Sociatis.Helpers;
using Sociatis.Models.Country;
using Sociatis.Models.Debug;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using WebServices;
using WebServices.BigParams.MarketOffers;
using WebServices.Helpers;
using WebServices.structs;

namespace Sociatis.Controllers
{
    public class DebugController : ControllerBase
    {
        private readonly ICountryRepository countryRepository;
        private readonly ICongressVotingService congressVotingService;
        private readonly ICongressVotingRepository congressVotingRepository;
        private readonly IBattleService battleService;
        private readonly IBattleRepository battleRepository;
        private readonly ICompanyService companyService;
        private readonly IWalletService walletService;
        private readonly IMarketService marketService;
        private readonly IEquipmentService equipmentService;
        private readonly ITransactionScopeProvider transactionScopeProvider;
        private readonly ICompanyRepository companyRepository;
        private readonly IEquipmentRepository equipmentRepository;
        private readonly IHotelRepository hotelRepository;
        private readonly IMahService mahService;
        private readonly IHouseService houseService;
        private readonly IHouseRepository houseRepository;
        private readonly HouseDayChangeProcessor houseDayChangeProcessor;

        public DebugController(IPopupService popupService, ICountryRepository countryRepository, ICongressVotingService congressVotingService,
            ICongressVotingRepository congressVotingRepository, IBattleService battleService, IBattleRepository battleRepository,
            ICompanyService companyService, IWalletService walletService, IMarketService marketService, IEquipmentService equipmentService,
            ITransactionScopeProvider transactionScopeProvider, ICompanyRepository companyRepository, IEquipmentRepository equipmentRepository,
            IHotelRepository hotelRepository, IMahService mahService, IHouseService houseService, IHouseRepository houseRepository,
            HouseDayChangeProcessor houseDayChangeProcessor) : base(popupService)
        {
            this.countryRepository = countryRepository;
            this.congressVotingService = congressVotingService;
            this.congressVotingRepository = congressVotingRepository;
            this.battleService = battleService;
            this.battleRepository = battleRepository;
            this.companyService = companyService;
            this.walletService = walletService;
            this.marketService = marketService;
            this.equipmentService = equipmentService;
            this.transactionScopeProvider = transactionScopeProvider;
            this.companyRepository = companyRepository;
            this.equipmentRepository = equipmentRepository;
            this.hotelRepository = hotelRepository;
            this.mahService = mahService;
            this.houseService = houseService;
            this.houseRepository = houseRepository;
            this.houseDayChangeProcessor = houseDayChangeProcessor;
        }

        [SociatisAuthorize(PlayerTypeEnum.Admin)]
        public ActionResult Misc()
        {
            string content = $"Default timeout = {TransactionManager.DefaultTimeout} <br/> " +
                $"Maximum timeout = {TransactionManager.MaximumTimeout}";

            return Content(content);
        }

        [SociatisAuthorize(PlayerTypeEnum.Admin)]
        public ActionResult DebugException()
        {
            Elmah.ErrorLog.GetDefault(null).Log(new Error(new Exception("test")));
            throw new Exception("dupa");
            return Content("a");
        }

        [SociatisAuthorize(PlayerTypeEnum.Admin)]
        [HttpGet]
        public ActionResult HotelDC()
        {
            return View();
        }

        [HttpPost]
        public ActionResult HotelDC(int hotelID)
        {
            var processor = new HotelDayChangeProcessor(equipmentService, equipmentRepository);
            var hotel = hotelRepository.GetById(hotelID);

            processor.ProcessHotel(GameHelper.CurrentDay + 1, hotel);
            equipmentRepository.SaveChanges();

            return View();
        }

        [SociatisAuthorize(PlayerTypeEnum.Admin)]
        public ActionResult CreateMahHotels()
        {
            mahService.CreateHotels();
            return RedirectBackWithSuccess("Done :D");
        }

        [SociatisAuthorize(PlayerTypeEnum.Admin)]
        public ActionResult CreateMahHouses()
        {
            mahService.CreateHouses();
            return RedirectBackWithSuccess("Done :D");
        }

        public ActionResult CreateMahCM()
        {
            mahService.CreateCM();
            return RedirectBackWithSuccess("Done :D");
        }

#if DEBUG

        [SociatisAuthorize(PlayerTypeEnum.Admin)]
        public ActionResult BuildHouse()
        {
            var citizen = SessionHelper.LoggedCitizen;

            houseService.CreateHouseForCitizen(citizen);
            return RedirectBackWithSuccess("Haha zbudowałem ci dom XDXDXDXDXDXD");
        }

        [SociatisAuthorize(PlayerTypeEnum.Admin)]
        public ActionResult DayChangeHouse(int houseID)
        {
            var house = houseRepository.GetById(houseID);
            houseDayChangeProcessor.ProcessDayChange(house, GameHelper.CurrentDay + 1);
            houseRepository.SaveChanges();
            return RedirectBackWithInfo("No i jak? Dom sie spierdolił przez ten czas XD?");
        }

        [SociatisAuthorize(PlayerTypeEnum.Admin)]
        [HttpGet]
        public ActionResult EndBattle()
        {
            return View();
        }

        [SociatisAuthorize(PlayerTypeEnum.Admin)]
        [HttpPost]
        public ActionResult EndBattle(int battleID)
        {
            var battle = battleRepository.GetById(battleID);
            battleService.EndBattle(battle);
            return View();
        }
        [SociatisAuthorize(PlayerTypeEnum.Admin)]
        [HttpGet]
        public ActionResult StupidCompanies()
        {
            ProductTypeEnum[] productTypes = new ProductTypeEnum[]
            {
                ProductTypeEnum.Bread,
                ProductTypeEnum.Weapon,
                ProductTypeEnum.Grain,
                ProductTypeEnum.Iron,
                ProductTypeEnum.Tea,
                ProductTypeEnum.Oil,
                ProductTypeEnum.Fuel,
                ProductTypeEnum.MovingTicket,
                ProductTypeEnum.TeaLeaf,
                ProductTypeEnum.Wood,
                ProductTypeEnum.UpgradePoints,
                ProductTypeEnum.MedicalSupplies,
                ProductTypeEnum.Paper,
                ProductTypeEnum.ConstructionMaterials

            };

            CurrencyTypeEnum[] currencies = new CurrencyTypeEnum[]
                {
                    CurrencyTypeEnum.PLN,
                    CurrencyTypeEnum.Gold,
                    CurrencyTypeEnum.Dollar
            };

            using (var trs = transactionScopeProvider.CreateTransactionScope())
            {
                foreach (var productType in productTypes)
                {
                    var name = RandomGenerator.GenerateString(50);
                    var company = companyService.CreateCompany(name, productType, SessionHelper.LoggedCitizen.RegionID, SessionHelper.LoggedCitizen.ID);
                    companyRepository.ReloadEntity(company);
                    companyRepository.ReloadNavigationProperty(company, x => x.Region);
                    foreach (var cur in currencies)
                    {
                        walletService.AddMoney(company.Entity.WalletID, new Money((int)cur, 1000));
                    }

                    equipmentService.GiveItem(productType, 1000, 1, company.Entity.Equipment);

                    marketService.AddOffer(new AddMarketOfferParameters()
                    {
                        Amount = 1000,
                        CompanyID = company.ID,
                        CountryID = company.Region.CountryID,
                        Price = 1,
                        ProductType = productType,
                        Quality = 1
                    });
                }
                var cname = RandomGenerator.GenerateString(50);
                var c = companyService.CreateCompany(cname, ProductTypeEnum.SellingPower, SessionHelper.LoggedCitizen.RegionID, SessionHelper.LoggedCitizen.ID);
                equipmentService.GiveItem(ProductTypeEnum.SellingPower, 1000, 1, c.Entity.Equipment);
                companyRepository.ReloadEntity(c);
                companyRepository.ReloadNavigationProperty(c, x => x.Region);
                foreach (var cur in currencies)
                {
                    walletService.AddMoney(c.Entity.WalletID, new Money((int)cur, 1000));
                }

                foreach (var productType in ProductGroups.Consumables)
                {
                    equipmentService.GiveItem(productType, 1000, 1, c.Entity.Equipment);

                    marketService.AddOffer(new AddMarketOfferParameters()
                    {
                        Amount = 1000,
                        CompanyID = c.ID,
                        CountryID = c.Region.CountryID,
                        Price = 1,
                        ProductType = productType,
                        Quality = 1
                    });
                }
                trs.Complete();
            }

            AddSuccess("Stupid companies completed");
            return RedirectBack();
        }
        [SociatisAuthorize(PlayerTypeEnum.Admin)]
        [HttpGet]
        public ActionResult GiveMeCongressman()
        {
            var citizen = SessionHelper.LoggedCitizen;

            var vm = new DebugAssignAsCongressman();
            return View(vm);
        }



        [SociatisAuthorize(PlayerTypeEnum.Admin)]
        [HttpPost]
        public ActionResult GiveMeCongressman(DebugAssignAsCongressman vm)
        {
            var citizen = SessionHelper.LoggedCitizen;

            if (citizen.Congressmen.Any(c => c.CountryID == vm.CountryID))
                return RedirectToHomeWithError("You actually are a congressman of this country!");

            if (ModelState.IsValid)
            {
                var country = countryRepository.GetById(vm.CountryID);

                country.Congressmen.Add(new Entities.Congressman()
                {
                    CitizenID = citizen.ID,
                    CountryID = country.ID,
                    LastVotingDay = 0
                });
                countryRepository.SaveChanges();
            }

            return View(vm);
        }



        [HttpGet]
        [SociatisAuthorize(PlayerTypeEnum.Admin)]
        public ActionResult GrantMePresident()
        {
            var countries = countryRepository.GetAll()
                .ToList()
                .Select(c => new CountryInfoViewModel(c));
            return View(countries);
        }

        [HttpPost]
        [SociatisAuthorize(PlayerTypeEnum.Admin)]
        public ActionResult GrantMePresident(int countryID)
        {
            var country = countryRepository.GetById(countryID);
            if (country == null)
                return RedirectBackWithError("O kurwa");

            if (country.President != null)
                return RedirectBackWithError("Już jest jakiś kurwiszon!");

            country.PresidentID = SessionHelper.LoggedCitizen.ID;
            countryRepository.SaveChanges();

            return RedirectBackWithSuccess("Udao sie!");
        }


        [HttpGet]
        [SociatisAuthorize(PlayerTypeEnum.Admin)]
        public ActionResult FinishCongressVoting()
        {
            var vm = new DebugFinishCongressVotingViewModel();

            return View(vm);
        }

        [HttpPost]
        [SociatisAuthorize(PlayerTypeEnum.Admin)]
        public ActionResult FinishCongressVoting(DebugFinishCongressVotingViewModel vm, string whatToDo)
        {
            if (ModelState.IsValid)
            {
                var voting = congressVotingRepository.GetById(vm.VotingID);
                if (whatToDo == "accept")
                    congressVotingService.FinishVoting(voting);
                else
                    congressVotingService.RejectVoting(voting, CongressVotingRejectionReasonEnum.NotEnoughVotes);
            }
            return View(vm);
        }
#endif
    }
}