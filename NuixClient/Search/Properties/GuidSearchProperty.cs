﻿using System;
using Orchestration;

namespace NuixClient.Search.Properties
{
    internal sealed class GuidSearchProperty : GenericSearchProperty<Guid>
    {
        public GuidSearchProperty(string propertyName) : base(propertyName)
        {
        }

        protected override Result<Guid> TryParse(string s)
        {
            if(Guid.TryParse(s, out var r))
                return Result<Guid>.Success(r);
            return Result<Guid>.Failure($"Could not parse '{s}' as a Guid.");
        }
    }
}