using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace EInvoice.Lib
{
    public class Customer : INotifyPropertyChanged
    {
        private string _name = string.Empty;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString() => Name;
    }

    public class Position : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private int _quantity;
        private decimal _amount;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged();
            }
        }

        public decimal Amount
        {
            get => _amount;
            set
            {
                _amount = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Invoice : INotifyPropertyChanged
    {
        private string _invoiceNumber = string.Empty;
        private DateTime _invoiceDate = DateTime.Today;
        private Customer? _selectedCustomer;
        private ObservableCollection<Position> _positions = new();

        public string InvoiceNumber
        {
            get => _invoiceNumber;
            set
            {
                _invoiceNumber = value;
                OnPropertyChanged();
            }
        }

        public DateTime InvoiceDate
        {
            get => _invoiceDate;
            set
            {
                _invoiceDate = value;
                OnPropertyChanged();
            }
        }

        public Customer? SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                _selectedCustomer = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Position> Positions
        {
            get => _positions;
            set
            {
                _positions = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}