﻿#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013-2016 Simple Injector Contributors
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
 * associated documentation files (the "Software"), to deal in the Software without restriction, including 
 * without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
 * copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the 
 * following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial 
 * portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT 
 * LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO 
 * EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER 
 * IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE 
 * USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

namespace SimpleInjector.Lifestyles
{
    using System;

    internal sealed class HybridLifestyle : Lifestyle
    {
        private readonly Predicate<Container> lifestyleSelector;
        private readonly Lifestyle trueLifestyle;
        private readonly Lifestyle falseLifestyle;

        internal HybridLifestyle(Predicate<Container> lifestyleSelector, Lifestyle trueLifestyle, Lifestyle falseLifestyle)
            : base("Hybrid " + GetHybridName(trueLifestyle) + " / " + GetHybridName(falseLifestyle))
        {
            this.lifestyleSelector = lifestyleSelector;
            this.trueLifestyle = trueLifestyle;
            this.falseLifestyle = falseLifestyle;
        }

        public override int Length
        {
            get { throw new NotSupportedException("The length property is not supported for this lifestyle."); }
        }

        internal override int ComponentLength(Container container) => 
            Math.Max(
                this.trueLifestyle.ComponentLength(container),
                this.falseLifestyle.ComponentLength(container));

        internal override int DependencyLength(Container container) => 
            Math.Min(
                this.trueLifestyle.DependencyLength(container),
                this.falseLifestyle.DependencyLength(container));

        internal static string GetHybridName(Lifestyle lifestyle)
        {
            var hybrid = lifestyle as HybridLifestyle;

            if (hybrid != null)
            {
                return hybrid.GetHybridName();
            }

            var scopedHybrid = lifestyle as ScopedHybridLifestyle;

            if (scopedHybrid != null)
            {
                return scopedHybrid.GetHybridName();
            }

            return lifestyle.Name;
        }

        protected override Registration CreateRegistrationCore<TConcrete>(Container container)
        {
            Func<bool> test = () => this.lifestyleSelector(container);

            return new HybridRegistration(typeof(TConcrete), test,
                this.trueLifestyle.CreateRegistration<TConcrete>(container),
                this.falseLifestyle.CreateRegistration<TConcrete>(container),
                this, container);
        }

        protected override Registration CreateRegistrationCore<TService>(Func<TService> instanceCreator,
            Container container)
        {
            Func<bool> test = () => this.lifestyleSelector(container);

            return new HybridRegistration(typeof(TService), test,
                this.trueLifestyle.CreateRegistration(instanceCreator, container),
                this.falseLifestyle.CreateRegistration(instanceCreator, container),
                this, container);
        }

        private string GetHybridName() => 
            GetHybridName(this.trueLifestyle) + " / " + GetHybridName(this.falseLifestyle);
    }
}