﻿using System.ComponentModel;
using Validar;

namespace WithGenericExternal
{
    [InjectValidation]
    public class Model : INotifyPropertyChanged
    {

        public string Property1;
        public string Property2;

        public event PropertyChangedEventHandler PropertyChanged;
    }
}