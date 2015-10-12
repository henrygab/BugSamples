using K = System.Diagnostics.Contracts;

/// <summary>
/// Building produces the following error:
/// Class1.cs(xx,13): warning CC1075:
///     CodeContracts: Contract class 'Sample1.ContractsForIFoo' references member
///     'Sample1.ContractsForIFoo.ValidateColumnUnchanged' which is not part of
///     the abstract class/interface being annotated.
/// There does not appear to be any way to markup the function in this class to
/// prevent this warning message.  None of the following attributes prevent this
/// warning either:
///     [ContractAbbreviator]    // This is the one that should work
///     [ContractRuntimeIgnored] // not intended purpose
///     [Pure]                   // Just for fun...
/// </summary>

namespace Sample1
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

        [K.ContractInvariantMethod()]
        private void Invariants()
        {
            K.Contract.Invariant(((IFoo)this).TotalColumns == 80);
            K.Contract.Invariant(((IFoo)this).CurrentColumn >= 0);
            K.Contract.Invariant(((IFoo)this).CurrentColumn < ((IFoo)this).TotalColumns);
        }
        [K.ContractAbbreviator]
        [K.Pure]
        //[K.ContractRuntimeIgnored]
        public void ValidateColumnUnchanged()
        {
            K.Contract.Ensures(
                K.Contract.OldValue<int>(((IFoo)this).CurrentColumn)
                == ((IFoo)this).CurrentColumn
                );
        }
        void IFoo.Bar()
        {
            // Should this warning emit at all on an internal abstract class
            // with the "ContractClassFor(typeof(IInterface)" attribute?
            ValidateColumnUnchanged();
            return;
        }
    }
}

