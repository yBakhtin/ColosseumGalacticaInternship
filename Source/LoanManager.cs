using System.Collections.Generic;
using UnityEngine;

namespace Colosseum.Management {
    public class LoanManager : SingletonInstance<LoanManager> {
        public List<Loan> loanOffers = new List<Loan>();
        public List<Loan> ownedLoans = new List<Loan>();
        public int maxLoanCapacity = 3;

        public List<Loan> LoanPool {
            get { return GameManager.Instance.content.loans; }
        }

        // Acquire given loan and pay the debt
        public bool AcquireLoan(Loan loan) {
            loanOffers.Remove(loan);
            InventoryManager.Instance.ModifyMoolah(loan.debt);
            ownedLoans.Add(loan);
            return true;
        }

        // Adds loan offer
        public bool AddLoanOffer(LoanType loanType) {
            if (loanOffers.Count == maxLoanCapacity)
                return false;

            Loan loan = null;
            for (int i = 0; i < LoanPool.Count; i++) {
                if (LoanPool[i].type == loanType) {
                    loan = LoanPool[i];
                    break;
                }
            }

            if (!loanOffers.Contains(loan)) {
                loanOffers.Add(loan);
            }

            return true;
        }
        
        // Pays off the loan
        public bool PayOff(Loan loan) {
            if (InventoryManager.Instance.Moolah < loan.debt) {
                Debug.Log("<color=green>LoanManager: Cannot pay off the loan! Not enough gold!</color>");
                return false;
            }

            InventoryManager.Instance.ModifyMoolah(-loan.debt);
            ownedLoans.Remove(loan);
            Debug.Log("<color=green>LoanManager: The loan is paid off!</color>");

            return true;
        }
    }
}
