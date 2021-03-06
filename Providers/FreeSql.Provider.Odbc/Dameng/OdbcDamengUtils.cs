﻿using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Odbc;
using System.Linq.Expressions;
using System.Text;

namespace FreeSql.Odbc.Dameng
{

    class OdbcDamengUtils : CommonUtils
    {
        public OdbcDamengUtils(IFreeSql orm) : base(orm)
        {
        }

        public override DbParameter AppendParamter(List<DbParameter> _params, string parameterName, ColumnInfo col, Type type, object value)
        {
            if (string.IsNullOrEmpty(parameterName)) parameterName = $"p_{_params?.Count}";
            var dbtype = (OdbcType)_orm.CodeFirst.GetDbInfo(type)?.type;
            switch (dbtype)
            {
                case OdbcType.Bit:
                    if (value == null) value = null;
                    else value = (bool)value == true ? 1 : 0;
                    dbtype = OdbcType.Int;
                    break;
               
                case OdbcType.Char:
                case OdbcType.NChar:
                case OdbcType.VarChar:
                case OdbcType.NVarChar:
                case OdbcType.Text:
                case OdbcType.NText:
                    value = string.Concat(value);
                    break;
            }
            var ret = new OdbcParameter { ParameterName = QuoteParamterName(parameterName), OdbcType = dbtype, Value = value };
            _params?.Add(ret);
            return ret;
        }

        public override DbParameter[] GetDbParamtersByObject(string sql, object obj) =>
            Utils.GetDbParamtersByObject<OdbcParameter>(sql, obj, null, (name, type, value) =>
            {
                var dbtype = (OdbcType)_orm.CodeFirst.GetDbInfo(type)?.type;
                switch (dbtype)
                {
                    case OdbcType.Bit:
                        if (value == null) value = null;
                        else value = (bool)value == true ? 1 : 0;
                        dbtype = OdbcType.Int;
                        break;

                    case OdbcType.Char:
                    case OdbcType.NChar:
                    case OdbcType.VarChar:
                    case OdbcType.NVarChar:
                    case OdbcType.Text:
                    case OdbcType.NText:
                        value = string.Concat(value);
                        break;
                }
                var ret = new OdbcParameter { ParameterName = $":{name}", OdbcType = dbtype, Value = value };
                return ret;
            });

        public override string FormatSql(string sql, params object[] args) => sql?.FormatOdbcOracle(args);
        public override string QuoteSqlName(string name)
        {
            var nametrim = name.Trim();
            if (nametrim.StartsWith("(") && nametrim.EndsWith(")"))
                return nametrim; //原生SQL
            return $"\"{nametrim.Trim('"').Replace(".", "\".\"")}\"";
        }
        public override string TrimQuoteSqlName(string name)
        {
            var nametrim = name.Trim();
            if (nametrim.StartsWith("(") && nametrim.EndsWith(")"))
                return nametrim; //原生SQL
            return $"{nametrim.Trim('"').Replace("\".\"", ".").Replace(".\"", ".")}";
        }
        public override string QuoteParamterName(string name) => $":{(_orm.CodeFirst.IsSyncStructureToLower ? name.ToLower() : name)}";
        public override string IsNull(string sql, object value) => $"nvl({sql}, {value})";
        public override string StringConcat(string[] objs, Type[] types) => $"{string.Join(" || ", objs)}";
        public override string Mod(string left, string right, Type leftType, Type rightType) => $"mod({left}, {right})";
        public override string Div(string left, string right, Type leftType, Type rightType) => $"trunc({left} / {right})";
        public override string Now => "systimestamp";
        public override string NowUtc => "getutcdate";

        public override string QuoteWriteParamter(Type type, string paramterName) => paramterName;
        public override string QuoteReadColumn(Type type, string columnName) => columnName;

        public override string GetNoneParamaterSqlValue(List<DbParameter> specialParams, Type type, object value)
        {
            if (value == null) return "NULL";
            if (type == typeof(byte[]))
            {
                var bytes = value as byte[];
                var sb = new StringBuilder().Append("rawtohex('0x");
                foreach (var vc in bytes)
                    sb.Append(vc.ToString("X").PadLeft(2, '0'));
                return sb.Append("')").ToString();
            }
            return FormatSql("{0}", value, 1);
        }
    }
}
