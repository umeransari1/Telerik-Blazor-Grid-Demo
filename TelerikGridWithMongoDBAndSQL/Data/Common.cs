using MongoDB.Bson;
using MongoDB.Driver;
using System.Reflection;
using System.Text.RegularExpressions;
using Telerik.DataSource;

namespace TelerikGridWithMongoDBAndSQL.Data
{
    public class Common
    {
        public IDictionary<string, bool> ProceesData<T>(T item) where T : class
        {
            IDictionary<string, bool> classProperties = new Dictionary<string, bool>();

            if (item != null)
            {
                Type type = item.GetType();

                PropertyInfo[] arrayPropertyInfos = type.GetProperties();
                foreach (PropertyInfo property in arrayPropertyInfos)
                {
                    classProperties.Add(property.Name, true);
                }
            }

            return classProperties;
        }

        public string DescriptorToSqlQuery(IList<IFilterDescriptor> filters, FilterCompositionLogicalOperator compositionOperator = FilterCompositionLogicalOperator.And)
        {
            string where = string.Empty;
            string filterString = string.Empty;
            int i = 1;
            try
            {
                if (!filters.Any()) return "";

                foreach (var filter in filters)
                {
                    if (filter is FilterDescriptor fd)
                    {
                        where += FilterWhereClause(fd);
                    }
                    else if (filter is CompositeFilterDescriptor cfd)
                    {
                        where += "(" + DescriptorToSqlQuery(cfd.FilterDescriptors, cfd.LogicalOperator) + ")";
                    }

                    if (i < filters.Count())
                    {
                        if (compositionOperator == FilterCompositionLogicalOperator.And)
                            where += " AND ";
                        else if (compositionOperator == FilterCompositionLogicalOperator.Or)
                            where += " OR ";
                        i++;
                    }
                }

            }
            catch
            {
                throw;
            }

            return where;
        }

        private string FilterWhereClause(FilterDescriptor fd)
        {
            string result = fd.Member;

            switch (fd.Operator)
            {
                case FilterOperator.IsLessThan:
                    if (fd.MemberType.FullName.Equals(typeof(DateTime).ToString()))
                        result = $"CAST({fd.Member} AS DATE) < CAST('{fd.Value}' AS DATE)";
                    else
                        result += $" < {fd.Value}"; 
                    break;
                case FilterOperator.IsLessThanOrEqualTo:
                    if (fd.MemberType.FullName.Equals(typeof(DateTime).ToString()))
                        result = $"CAST({fd.Member} AS DATE) <= CAST('{fd.Value}' AS DATE)";
                    else
                        result += $" <= {fd.Value}";
                    break;
                case FilterOperator.IsGreaterThan:
                    if (fd.MemberType.FullName.Equals(typeof(DateTime).ToString()))
                        result = $"CAST({fd.Member} AS DATE) > CAST('{fd.Value}' AS DATE)";
                    else
                        result += $" > {fd.Value}";
                    break;
                case FilterOperator.IsGreaterThanOrEqualTo:
                    if (fd.MemberType.FullName.Equals(typeof(DateTime).ToString()))
                        result = $"CAST({fd.Member} AS DATE) >= CAST('{fd.Value}' AS DATE)";
                    else
                        result += $" >= {fd.Value}"; 
                    break;
                case FilterOperator.IsEqualTo:
                    if (fd.MemberType.FullName.Equals(typeof(DateTime).ToString()))
                        result = $"CAST({fd.Member} AS DATE) = CAST('{fd.Value}' AS DATE)";
                    else
                        result += $" = '{fd.Value?.ToString().Replace("'", "''")}'";
                    break;
                case FilterOperator.IsNotEqualTo:
                    if (fd.MemberType.FullName.Equals(typeof(DateTime).ToString()))
                        result = $"(CAST({fd.Member} AS DATE) <> CAST('{fd.Value}' AS DATE) OR {fd.Member} IS NULL)";
                    else
                        result += $" <> '{fd.Value?.ToString().Replace("'", "''")}'";
                    break;
                case FilterOperator.Contains: result += $" like '%{fd.Value?.ToString().Replace("'", "''")}%' "; break;
                case FilterOperator.DoesNotContain: result += $" not like '%{fd.Value?.ToString().Replace("'", "''")}%' "; break;
                case FilterOperator.StartsWith: result += $" like '{fd.Value?.ToString().Replace("'", "''")}%' "; break;
                case FilterOperator.EndsWith: result += $" like '%{fd.Value?.ToString().Replace("'", "''")}' "; break;
                case FilterOperator.IsNull: result += $" IS NULL "; break;
                case FilterOperator.IsNotNull: result += $" IS NOT NULL "; break;
                case FilterOperator.IsEmpty: result += $" = '' "; break;
                case FilterOperator.IsNotEmpty: result += $" <> '' "; break;
                case FilterOperator.IsNullOrEmpty: result += $" IS NULL OR {fd.Member} = '' "; break;
                case FilterOperator.IsNotNullOrEmpty: result += $" IS NOT NULL AND {fd.Member} <> '' "; break;
                default: result = ""; break;
            }

            result = string.IsNullOrEmpty(result) ? "" : result;
            return result;
        }

        public FilterDefinition<T> DesccriptorToMongoDBFilter<T>(IList<IFilterDescriptor> filters) where T : class
        {
            var filterBuilder = Builders<T>.Filter;
            var filter = filterBuilder.Empty;
            try
            {
                if (!filters.Any()) return filter;

                foreach (var flt in filters)
                {
                    if (flt is FilterDescriptor fd)
                    {
                        filter &= MongoDBWhereClause<T>(fd);
                    }
                    else if (flt is CompositeFilterDescriptor cfd)
                    {
                        FilterDefinition<T>[] filtersArray = GetFilterDefinitionArray<T>(cfd);

                        if (cfd.LogicalOperator == FilterCompositionLogicalOperator.And)
                            filter &= filterBuilder.And(filtersArray);
                        else if (cfd.LogicalOperator == FilterCompositionLogicalOperator.Or)
                            filter &= filterBuilder.Or(filtersArray);
                    }
                }

            }
            catch
            {
                throw;
            }

            return filter;
        }

        private FilterDefinition<T>[] GetFilterDefinitionArray<T>(CompositeFilterDescriptor cfd) where T : class
        {
            FilterDefinition<T>[] filterDefinitions = new FilterDefinition<T>[cfd.FilterDescriptors.Count];

            int i = 0;
            foreach(var fd in cfd.FilterDescriptors)
            {
                FilterDescriptor descriptor = fd as FilterDescriptor;
                filterDefinitions[i] = MongoDBWhereClause<T>(descriptor);
                i++;
            }

            return filterDefinitions;
        }

        private FilterDefinition<T> MongoDBWhereClause<T>(FilterDescriptor descriptor) where T : class
        {
            var filterBuilder = Builders<T>.Filter;
            var filter = filterBuilder.Empty;
            string escapeText = "";
            
            if(descriptor.Value != null)
                escapeText = Regex.Escape(descriptor.Value?.ToString());

            DateTime date = new DateTime();
            DateTime start = new DateTime();

            if (descriptor.MemberType.FullName.Equals(typeof(DateTime).ToString()) && !string.IsNullOrEmpty(descriptor.Value?.ToString()))
            {
                DateTime.TryParse(descriptor.Value.ToString(), out date);
                start = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
            }

            switch (descriptor.Operator)
            {
                case FilterOperator.IsEqualTo:
                    if (descriptor.MemberType.FullName.Equals(typeof(DateTime).ToString()))
                        filter &= filterBuilder.Eq(descriptor.Member.ToString(), new BsonDateTime(start));
                    else
                        filter &= filterBuilder.Eq(descriptor.Member.ToString(), descriptor.Value);
                    break;
                case FilterOperator.IsNotEqualTo:
                    if (descriptor.MemberType.FullName.Equals(typeof(DateTime).ToString()))
                        filter &= filterBuilder.Ne(descriptor.Member.ToString(), new BsonDateTime(start));
                    else
                        filter &= filterBuilder.Ne(descriptor.Member.ToString(), descriptor.Value);
                    break;
                case FilterOperator.IsGreaterThan:
                    if (descriptor.MemberType.FullName.Equals(typeof(DateTime).ToString()))
                        filter &= filterBuilder.Gt(descriptor.Member.ToString(), new BsonDateTime(start));
                    else
                        filter &= filterBuilder.Gt(descriptor.Member.ToString(), descriptor.Value);
                    break;
                case FilterOperator.IsGreaterThanOrEqualTo:
                    if (descriptor.MemberType.FullName.Equals(typeof(DateTime).ToString()))
                        filter &= filterBuilder.Gte(descriptor.Member.ToString(), new BsonDateTime(start));
                    else
                        filter &= filterBuilder.Gte(descriptor.Member.ToString(), descriptor.Value);
                    break;
                case FilterOperator.IsLessThan:
                    if (descriptor.MemberType.FullName.Equals(typeof(DateTime).ToString()))
                        filter &= filterBuilder.Lt(descriptor.Member.ToString(), new BsonDateTime(start));
                    else
                        filter &= filterBuilder.Lt(descriptor.Member.ToString(), descriptor.Value);
                    break;
                case FilterOperator.IsLessThanOrEqualTo:
                    if (descriptor.MemberType.FullName.Equals(typeof(DateTime).ToString()))
                        filter &= filterBuilder.Lte(descriptor.Member.ToString(), new BsonDateTime(start));
                    else
                        filter &= filterBuilder.Lte(descriptor.Member.ToString(), descriptor.Value);
                    break;
                case FilterOperator.Contains:
                    var regex = new Regex(escapeText, RegexOptions.IgnoreCase);
                    filter &= filterBuilder.Regex(descriptor.Member.ToString(), BsonRegularExpression.Create(regex));
                    break;
                case FilterOperator.DoesNotContain:
                    string expression = "^(?!.*" + escapeText + ").*$";
                    var notCOntainsRegex = new Regex(expression, RegexOptions.IgnoreCase);
                    filter &= filterBuilder.Regex(descriptor.Member.ToString(), new BsonRegularExpression(notCOntainsRegex));
                    break;
                case FilterOperator.StartsWith:
                    string startWithExpression = "^" + escapeText;
                    var startWithRegex = new Regex(startWithExpression, RegexOptions.IgnoreCase);
                    filter &= filterBuilder.Regex(descriptor.Member.ToString(), new BsonRegularExpression(startWithRegex));
                    break;
                case FilterOperator.EndsWith:
                    string endWithExpression = escapeText + "$";
                    var endWithRegex = new Regex(endWithExpression, RegexOptions.IgnoreCase);
                    filter &= filterBuilder.Regex(descriptor.Member.ToString(), new BsonRegularExpression(endWithRegex));
                    break;
                case FilterOperator.IsNull:
                    filter &= filterBuilder.Exists(descriptor.Member.ToString(), false);
                    break;
                case FilterOperator.IsNotNull:
                    filter &= filterBuilder.Exists(descriptor.Member.ToString(), true);
                    break;
                case FilterOperator.IsEmpty:
                    filter &= filterBuilder.Eq(descriptor.Member.ToString(), "");
                    break;
                case FilterOperator.IsNotEmpty:
                    filter &= filterBuilder.Ne(descriptor.Member.ToString(), "");
                    break;
                case FilterOperator.IsNullOrEmpty:
                    filter &= filterBuilder.Or(filterBuilder.Exists(descriptor.Member.ToString(), false), filterBuilder.Eq(descriptor.Member.ToString(), ""));
                    break;
                case FilterOperator.IsNotNullOrEmpty:
                    filter &= filterBuilder.And(filterBuilder.Exists(descriptor.Member.ToString(), true), filterBuilder.Ne(descriptor.Member.ToString(), ""));
                    break;
                default: break;
            }

            return filter;
        }
    }
}
