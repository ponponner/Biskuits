using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;

namespace Biskuits.Helpers.Extensions
{
    public static class ReflectionExtensions
    {
        public static Type GetPropertyType<TSource, TProperty>(this TSource source, Expression<Func<TSource, TProperty>> propertyExpr)
        {
            Contract.Requires(propertyExpr != null);

            var body = propertyExpr.Body as MemberExpression;
            if (body == null)
                throw new ArgumentException("式がプロパティを参照していません。", nameof(propertyExpr));

            var propertyInfo = body.Member as PropertyInfo;
            if (propertyInfo == null)
                throw new ArgumentException("式がプロパティを参照していません。", nameof(propertyExpr));

            return propertyInfo.PropertyType;
        }
    }
}
