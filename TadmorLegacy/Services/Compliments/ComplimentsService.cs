using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Microsoft.Extensions.Options;

namespace Tadmor.Services.Compliments
{
    [SingletonService]
    public class ComplimentsService
    {
        public IReadOnlyCollection<string> Compliments { get; }

        public ComplimentsService(IOptionsSnapshot<ComplimentsOptions> options)
        {
            Compliments = options.Value.Compliments.AsReadOnly();
        }


    }
}
