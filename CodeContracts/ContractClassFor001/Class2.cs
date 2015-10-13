using System;
using K = System.Diagnostics.Contracts;

/// <summary>
/// Two bugs shown in this simple example.
/// 
/// Issue:
/// Contracts within ContractInvariantMethod method fail to be inherited via autoproperties in ContractClassFor() class
/// 
/// Steps to Repro:
/// Build this project.  :)
/// 
/// Expected Results:
/// Per section 2.3.1 of Code Contracts Specification:
///     Invariants on automatic properties SHALL add Ensures/Requires
///     to the corresponding Get/Set.
/// Therefore, the expectation is that the Ensures/Requires
/// corresponding to anything listed in a ContractInvariantMethod attributed
/// function will be applied to the autoproperties, and thus inherited for
/// any implementation.
///
/// Actual Results:
/// Code Contracts fails to detect / report any problems with the below code,
/// which suggests either the contract is not being detected at all, or is
/// somehow getting lost....
/// 
/// 
/// 
/// 
/// 
/// Issue:
/// Contracts within ContractAbbreviator method fail to be inherited from ContractClassFor() class
/// 
/// Steps to Repro:
/// Build this project.  :)
/// 
/// Expected Results:
/// Contracts indirectly called via a ContractAbbreviator method are properly inherited.
///
/// Actual Results:
/// Code Contracts fails to detect / report any problems with the below code,
/// which suggests either the contract is not being detected at all, or is
/// somehow getting lost....
/// 
/// 
/// 
/// </summary>

namespace ContractClassFor001
{
    [K.ContractClass(typeof(ContractsForIFoo))]
    public interface IFoo
    {
        int TotalColumns { get; }
        int CurrentColumn { get; set; }
        void Bar();
    }

    [K.ContractClassFor(typeof(IFoo))]
    internal abstract class ContractsForIFoo : IFoo
    {

        int IFoo.CurrentColumn
        {
            get;
            set;
        }
        int IFoo.TotalColumns
        {
            get;
        }

        /// <summary>
        /// Per section 2.3.1 of Code Contracts Specification,
        /// Invariants on automatic properties will add Ensures/Requires
        /// to the corresponding Get/Set.
        /// ***** This functionality FAILS TO WORK for ContractClassFor() classes *****
        /// </summary>
        [K.ContractInvariantMethod()]
        private void Invariants()
        {
            K.Contract.Invariant(((IFoo)this).TotalColumns == 80);
            K.Contract.Invariant(((IFoo)this).CurrentColumn >= 0);
            K.Contract.Invariant(((IFoo)this).CurrentColumn < ((IFoo)this).TotalColumns);
        }
        [K.ContractAbbreviator]
        [K.Pure]
        public void ValidateColumnUnchanged()
        {
            K.Contract.Ensures(
                K.Contract.OldValue<int>(((IFoo)this).CurrentColumn)
                == ((IFoo)this).CurrentColumn
                );
        }
        void IFoo.Bar()
        {
            // NOTE: can uncomment these two lines to prove that inline code contracts are still being inherited
            // K.Contract.Requires(((IFoo)this).CurrentColumn == 0);
            // K.Contract.Ensures(((IFoo)this).CurrentColumn == 0);

            // Should this warning emit at all on an internal abstract class
            // with the "ContractClassFor(typeof(IInterface)" attribute?
            ValidateColumnUnchanged();
            return;
        }
    }


    public sealed partial class Foo : IFoo
    {
        private int m_CurrentColumn;
        private int m_TotalColumns;

        public Foo()
        {
            m_CurrentColumn = 0;

            // the following line should cause a contract validation failure.
            // the contract is found in class ContractsForIFoo, which has attribute [ContractClassFor(typeof(IFoo))]
            //      See the Invariants() function, which has attribute [ContractInvariantMethod]
            //      and which internally specifies that TotalColumns must always be exactly 80.
            m_TotalColumns = 1000;
        }

        public int CurrentColumn
        {
            get
            {
                return m_CurrentColumn;
            }

            set
            {
                // this is not guaranteed by default code contracts(!)
                m_CurrentColumn = value;
            }
        }

        public int TotalColumns
        {
            get
            {
                return m_TotalColumns;
            }
            private set
            {
                m_TotalColumns = value;
            }

        }

        public void Bar()
        {
            // the following line should cause a contract validation failure.
            //      See class ContractsForIFoo, which has attribute [ContractClassFor(typeof(IFoo))]
            //      See ContractsForIFoo.Invariants(), which has attribute [ContractInvariantMethod]
            //          and which includes the contract that TotalColumns must always be exactly 80.
            this.TotalColumns = 120;

            // the following line should cause a contract validation failure:
            //      See class ContractsForIFoo, specifically the attribute [ContractClassFor(typeof(IFoo))]
            //      See ContractsForIFoo.Bar(), which calls ValidateColumnUnchanged().
            //      ValidateColumnUnchanged() has attribute [ContractAbbreviator], which
            //          specifies that CurrentColumn SHALL NOT be modified by Bar().
            //
            //      Accordingly, the ValidateColumnUnchanged() contract should "flow through" 
            //          to the Bar() function that calls it
            //      Because of code contract inheritance, the below line should be flagged as an error
            //          for violating the contracts specified in ValidateColumnUnchanged().
            this.CurrentColumn = Math.Min(50, m_CurrentColumn + 8);
        }

        public void DoSomethingElse()
        {
            var t = new Foo();
            t.Bar();
            t.CurrentColumn = 10;
            t.Bar();
            t.CurrentColumn = 100;
            t.Bar();
            t.CurrentColumn = 0;
            t.Bar();
        }
    }

}

