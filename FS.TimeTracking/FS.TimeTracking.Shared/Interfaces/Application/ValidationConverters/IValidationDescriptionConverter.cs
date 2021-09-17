﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace FS.TimeTracking.Shared.Interfaces.Application.ValidationConverters
{
    /// <summary>
    /// Validation description converter
    /// </summary>
    public interface IValidationDescriptionConverter
    {
        /// <summary>
        /// Gets the validation attributes convertible to descriptions.
        /// </summary>
        IEnumerable<Type> SupportedValidationAttributes { get; }

        /// <summary>
        /// Converts the specified attribute to a validation description.
        /// </summary>
        /// <param name="attribute">The attribute to convert.</param>
        /// <param name="errorI18NPrefix">The error i18n prefix.</param>
        JObject Convert(CustomAttributeData attribute, string errorI18NPrefix);
    }
}