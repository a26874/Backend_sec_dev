/*
*	<copyright file="SqlHelpers">
*	</copyright>
* 	<author>Marco Macedo</author>
*	<contact>a26874@alunos.ipca.pt</contact>
*   <date>2025 12/13/2025 11:38:24 PM</date>
*	<description></description>
**/

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Backend_sec_dev.Shared.Helpers
{
    public static class SqlHelpers
    {

        public static string SqlExceptionError(Exception ex)
        {
            string message = string.Empty;
            switch (ex)
            {
                case DbUpdateException dbUpdateEx:
                    message = TreatSqlExceptionError(dbUpdateEx);
                    if (NullChecks.StringNullOrEmpty(message)) 
                        return ex.Message;
                    return message;
                default:
                    break;
            }
            return message;
        }

        private static string TreatSqlExceptionError(DbUpdateException dbUpdateEx)
        {
            string message = string.Empty;

            if (dbUpdateEx.InnerException is SqlException ex)
            {
                int errorNumber = ex.Number;

                switch (errorNumber)
                {
                    case 2627:
                        message = GetColumnConstraint(ex);
                        return message;
                    default:
                        break;
                }

            }
            return message;
        }

        private static string GetColumnConstraint(SqlException ex)
        {
            if (!NullChecks.StringNullOrEmpty(ex.Message) && ex.Message.Contains("UQ_Users_Email"))
            {
                return "A user with the same email already exists, please try another one.";
            }
            return string.Empty;
        }
    }
}