﻿using Castr.Exceptions;
using Castr.Helpers;
using Castr.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Castr
{
    public class CastrClass<TExistingClass> : ICastr
    {
        private readonly TExistingClass _existingClass;
        private readonly ClassOptions _classOptions;

        public CastrClass(TExistingClass existingClass,
            ClassOptions classOptions)
        {
            _existingClass = existingClass;
            _classOptions = classOptions;
        }

        public CastrClass(TExistingClass existingClass)
        {
            _existingClass = existingClass;
            _classOptions = new ClassOptions() { IsStrict = false };
        }

        public TNewClass CastAsClass<TNewClass>() where TNewClass : class
        {
            if (_classOptions.IsStrict
                && !ClassHelper.AreClassesRelated<TExistingClass, TNewClass>())
            {
                throw new CastingException("Only related classes can be cast where option strict is set");
            }

            var newObject = Activator.CreateInstance<TNewClass>();
            var newProps = typeof(TNewClass).GetProperties();

            foreach (var prop in newProps)
            {
                if (!prop.CanWrite) continue;

                var existingPropertyInfo = typeof(TExistingClass).GetProperty(prop.Name);
                if (existingPropertyInfo == null || !existingPropertyInfo.CanRead) continue;
                var value = existingPropertyInfo.GetValue(_existingClass);
                
                prop.SetValue(newObject, value, null);
            }

            return newObject;
        }

        public T CastAsStruct<T>() where T : struct
        {
            throw new NotImplementedException();
        }

        public T ExtractField<T>(string name)
        {
            var prop = typeof(TExistingClass).GetProperty(name);

            if (prop == null)
            {
                throw new InvalidFieldException($"Unable to find {name}");
            }

            return (T)prop.GetValue(_existingClass);
        }

        public Dictionary<string, object> CastAsDictionary()
        {
            Dictionary<string, object> returnDictionary = new Dictionary<string, object>();
            var props = typeof(TExistingClass).GetProperties();

            foreach (var prop in props)
            {
                var existingPropertyInfo = typeof(TExistingClass).GetProperty(prop.Name);
                if (existingPropertyInfo == null || !existingPropertyInfo.CanRead) continue;
                var value = existingPropertyInfo.GetValue(_existingClass);

                returnDictionary.Add(prop.Name, value);
            }

            return returnDictionary;
        }
    }
}
