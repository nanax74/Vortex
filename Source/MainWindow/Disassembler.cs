using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vortex
{
    public class KernelEntry : INotifyPropertyChanged
    {
        private uint _address;
        private uint _value;
        private string? _mnemonic;
        private string? _operands;

        public uint Address
        {
            get => _address;
            set
            {
                if (_address != value)
                {
                    _address = value;
                    OnPropertyChanged(nameof(Address));
                }
            }
        }

        public uint Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged(nameof(Value));
                }
            }
        }

        public string? Mnemonic
        {
            get => _mnemonic;
            set
            {
                if (_mnemonic != value)
                {
                    _mnemonic = value;
                    OnPropertyChanged(nameof(Mnemonic));
                }
            }
        }

        public string? Operands
        {
            get => _operands;
            set
            {
                if (_operands != value)
                {
                    _operands = value;
                    OnPropertyChanged(nameof(Operands));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}