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
    public class SalaryCostFinance : CompanyFinanceBase
    {
        public SalaryCostFinance(decimal total, int currencyID) : base(total, currencyID)
        {
        }

        public override CompanyFinanceTypeEnum FinanceType => CompanyFinanceTypeEnum.SalaryCost;

        public override void Modify(CompanyFinanceSummary summary)
        {
            Debug.Assert(summary.CurrencyID == CurrencyID);

            summary.SalaryCost += Total;
        }
    }
}
