using SAP.API.Tests.Controllers;

namespace SAP.API.Tests;

public static class TestRunner
{
    public static async Task<int> Main(string[] args)
    {
        Console.WriteLine("🧪 Running SAP API Integration Tests...\n");

        try
        {
            // Run Accounts Controller Tests
            Console.WriteLine("📊 Testing AccountsController...");
            using var accountTests = new AccountsControllerTests();
            var accountResults = await accountTests.RunAllTests();
            Console.WriteLine($"   Result: {(accountResults ? "✅ PASSED" : "❌ FAILED")}\n");

            // Run Journal Entries Controller Tests
            Console.WriteLine("📝 Testing JournalEntriesController...");
            using var journalTests = new JournalEntriesControllerTests();
            var journalResults = await journalTests.RunAllTests();
            Console.WriteLine($"   Result: {(journalResults ? "✅ PASSED" : "❌ FAILED")}\n");

            // Overall Results
            var allPassed = accountResults && journalResults;
            Console.WriteLine("🏆 Overall Test Results:");
            Console.WriteLine($"   Accounts API: {(accountResults ? "✅ PASSED" : "❌ FAILED")}");
            Console.WriteLine($"   Journal Entries API: {(journalResults ? "✅ PASSED" : "❌ FAILED")}");
            Console.WriteLine($"   Overall: {(allPassed ? "✅ ALL TESTS PASSED" : "❌ SOME TESTS FAILED")}");

            return allPassed ? 0 : 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Test execution failed: {ex.Message}");
            Console.WriteLine($"   Stack Trace: {ex.StackTrace}");
            return 1;
        }
    }
} 