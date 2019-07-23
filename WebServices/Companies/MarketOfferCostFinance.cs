﻿using Entities;
using Entities.enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServices.Companies
{
    public class MarketOfferCostFinance : CompanyFinanceBase
    {
        public MarketOfferCostFinance(decimal total, int currencyID) : base(total, currencyID)
        {
        }

        public override CompanyFinanceTypeEnum FinanceType => CompanyFinanceTypeEnum.MarketOfferCost;

        public override void Modify(CompanyFinanceSummary summary)
        {
            Debug.Assert(summary.CurrencyID == CurrencyID);

            summary.MarketOfferCost += Total;
        }
    }
}
