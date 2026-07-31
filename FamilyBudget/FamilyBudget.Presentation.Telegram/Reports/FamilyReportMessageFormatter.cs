using FamilyBudget.Application.Dtos;

namespace FamilyBudget.Presentation.Telegram.Reports;

public static class FamilyReportMessageFormatter
{
    public static string Format(FamilyReportDto report, string periodTitle)
    {
        var topCategory = report.TopCategory is null
            ? "Нет расходов"
            : $"{CategoryName(report.TopCategory)} — {report.TopCategoryAmount:0.00} BYN";
        var categories = report.Categories.Count == 0
            ? "Нет расходов"
            : string.Join(
                '\n',
                report.Categories.Select(category =>
                    $"• {CategoryName(category.Category.ToString())}: " +
                    $"{category.Amount:0.00} BYN ({category.Percent:0.00} %)"));
        var members = report.Members.Count == 0
            ? "Нет участников"
            : string.Join(
                "\n\n",
                report.Members.Select(member =>
                    $"👤 {member.MemberName}\n" +
                    $"  Доходы: {member.TotalIncomes:0.00} BYN\n" +
                    $"  Расходы: {member.TotalExpenses:0.00} BYN\n" +
                    $"  Разница: {member.NetAmount:0.00} BYN"));

        return $"""
               📊 Семейный отчёт {periodTitle}

               💰 Доходы: {report.TotalIncome:0.00} BYN
               💸 Расходы: {report.TotalExpenses:0.00} BYN
               📈 Разница: {report.NetAmount:0.00} BYN
               📉 Доля расходов от доходов: {report.ExpensePercent:0.00} %

               🏆 Больше всего потрачено:
               {topCategory}

               📂 Расходы по категориям:
               {categories}

               👥 Статистика участников:
               {members}

               🏦 Копилка:
               ➕ Внесено за период: {report.Savings.PeriodContributions:0.00} BYN
               ➖ Снято за период: {report.Savings.PeriodWithdrawals:0.00} BYN
               📊 Изменение за период: {report.Savings.PeriodNetChange:0.00} BYN
               💰 Текущий остаток: {report.Savings.CurrentBalance:0.00} BYN
               """;
    }

    private static string CategoryName(string category) => category switch
    {
        "Food" => "Еда",
        "Transport" => "Транспорт",
        "Entertainment" => "Развлечения",
        "Health" => "Здоровье",
        "Home" => "Дом",
        _ => "Другое"
    };
}
