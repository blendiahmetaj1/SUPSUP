﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Entities.enums;

namespace Sociatis.Models.Congress.Votings
{
    public class ViewChangePartyCreateFeeViewModel : ViewVotingBaseViewModel
    {
        public double CreationFee { get; set; }
        public string CurrencySymbol { get; set; }


        public ViewChangePartyCreateFeeViewModel(Entities.CongressVoting voting, bool isPlayerCongressman, bool canVote)
            :base(voting, isPlayerCongressman, canVote)
        {
            CreationFee = double.Parse(voting.Argument1);
            CurrencySymbol = voting.Country.Currency.Symbol;
        }
    }
}