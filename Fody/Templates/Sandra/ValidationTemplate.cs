using System;
using System.Collections;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using Sandra.SimpleValidator;
using Scalpel;

namespace Templates.Sandra
{

    [Remove]
    public class ValidationTemplate : 
        IDataErrorInfo, 
        INotifyDataErrorInfo
    {
        INotifyPropertyChanged target;
        IModelValidator validator;
        ValidationResult validationResult;

        public ValidationTemplate(INotifyPropertyChanged target)
        {
            this.target = target;
            validator = GetValidator(target.GetType());
            validationResult = validator.Validate(target);
            target.PropertyChanged += Validate;
        }

        static ConcurrentDictionary<RuntimeTypeHandle, IModelValidator> validators = new ConcurrentDictionary<RuntimeTypeHandle, IModelValidator>();

        static IModelValidator GetValidator(Type modelType)
        {
            IModelValidator validator;
            if (!validators.TryGetValue(modelType.TypeHandle, out validator))
            {
                var typeName = $"{modelType.Namespace}.{modelType.Name}Validator";
                var type = modelType.Assembly.GetType(typeName, true);
                validators[modelType.TypeHandle] = validator = (IModelValidator) Activator.CreateInstance(type);
            }
            return validator;
        }

        void Validate(object sender, PropertyChangedEventArgs e)
        {
            validationResult = validator.Validate(target);
            foreach (var error in validationResult.Messages)
            {
                RaiseErrorsChanged(error.PropertyName);
            }
        }

        public IEnumerable GetErrors(string propertyName)
        {
            return validationResult.Messages
                                   .Where(x => x.PropertyName == propertyName)
                                   .Select(x => x.Message);
        }

        public bool HasErrors => validationResult.IsInvalid;

        public string Error
        {
            get
            {
                var strings = validationResult.Messages.Select(x => x.Message)
                                              .ToArray();
                return string.Join(Environment.NewLine, strings);
            }
        }

        public string this[string propertyName]
        {
            get
            {
                var strings = validationResult.Messages.Where(x => x.PropertyName == propertyName)
                                              .Select(x => x.Message)
                                              .ToArray();
                return string.Join(Environment.NewLine, strings);
            }
        }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        void RaiseErrorsChanged(string propertyName)
        {
            var handler = ErrorsChanged;
            handler?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
    }
}