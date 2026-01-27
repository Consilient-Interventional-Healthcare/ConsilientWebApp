using Consilient.ProviderAssignments.Services.Import.Transformers;

namespace Consilient.Infrastructure.ExcelImporter.Tests.Unit;

[TestClass]
public class NameParserTests
{
    #region SplitPatientName Tests

    [TestMethod]
    public void SplitPatientName_LastCommaFirst_SplitsCorrectly()
    {
        var (lastName, firstName) = NameParser.SplitPatientName("Smith, John");

        Assert.AreEqual("Smith", lastName);
        Assert.AreEqual("John", firstName);
    }

    [TestMethod]
    public void SplitPatientName_FirstLast_SplitsCorrectly()
    {
        var (lastName, firstName) = NameParser.SplitPatientName("John Smith");

        Assert.AreEqual("Smith", lastName);
        Assert.AreEqual("John", firstName);
    }

    [TestMethod]
    public void SplitPatientName_SingleName_TreatsAsLastName()
    {
        var (lastName, firstName) = NameParser.SplitPatientName("Smith");

        Assert.AreEqual("Smith", lastName);
        Assert.AreEqual("", firstName);
    }

    [TestMethod]
    public void SplitPatientName_NullOrEmpty_ReturnsEmpty()
    {
        var (lastName1, firstName1) = NameParser.SplitPatientName(null);
        var (lastName2, firstName2) = NameParser.SplitPatientName("");
        var (lastName3, firstName3) = NameParser.SplitPatientName("   ");

        Assert.AreEqual("", lastName1);
        Assert.AreEqual("", firstName1);
        Assert.AreEqual("", lastName2);
        Assert.AreEqual("", firstName2);
        Assert.AreEqual("", lastName3);
        Assert.AreEqual("", firstName3);
    }

    [TestMethod]
    public void SplitPatientName_WithExtraSpaces_TrimsResults()
    {
        var (lastName, firstName) = NameParser.SplitPatientName("  Smith  ,  John  ");

        Assert.AreEqual("Smith", lastName);
        Assert.AreEqual("John", firstName);
    }

    #endregion

    #region NormalizeCase Tests

    [TestMethod]
    public void NormalizeCase_SimpleNameUppercase_ReturnsProperCase()
    {
        var result = NameParser.NormalizeCase("SMITH");

        Assert.AreEqual("Smith", result);
    }

    [TestMethod]
    public void NormalizeCase_SimpleNameLowercase_ReturnsProperCase()
    {
        var result = NameParser.NormalizeCase("smith");

        Assert.AreEqual("Smith", result);
    }

    [TestMethod]
    public void NormalizeCase_OBrien_HandlesApostrophe()
    {
        var result = NameParser.NormalizeCase("O'BRIEN");

        Assert.AreEqual("O'Brien", result);
    }

    [TestMethod]
    public void NormalizeCase_DAngelo_HandlesApostrophe()
    {
        var result = NameParser.NormalizeCase("d'angelo");

        Assert.AreEqual("D'Angelo", result);
    }

    [TestMethod]
    public void NormalizeCase_McDonald_HandlesMcPrefix()
    {
        var result = NameParser.NormalizeCase("MCDONALD");

        Assert.AreEqual("McDonald", result);
    }

    [TestMethod]
    public void NormalizeCase_McGregor_HandlesMcPrefix()
    {
        var result = NameParser.NormalizeCase("mcgregor");

        Assert.AreEqual("McGregor", result);
    }

    [TestMethod]
    public void NormalizeCase_MacArthur_HandlesMacPrefix()
    {
        var result = NameParser.NormalizeCase("MACARTHUR");

        Assert.AreEqual("MacArthur", result);
    }

    [TestMethod]
    public void NormalizeCase_MacDowell_HandlesMacPrefix()
    {
        var result = NameParser.NormalizeCase("macdowell");

        Assert.AreEqual("MacDowell", result);
    }

    [TestMethod]
    public void NormalizeCase_Mack_DoesNotTreatAsMacPrefix()
    {
        var result = NameParser.NormalizeCase("MACK");

        Assert.AreEqual("Mack", result);
    }

    [TestMethod]
    public void NormalizeCase_Macey_DoesNotTreatAsMacPrefix()
    {
        var result = NameParser.NormalizeCase("MACEY");

        Assert.AreEqual("Macey", result);
    }

    [TestMethod]
    public void NormalizeCase_HyphenatedName_HandlesBothParts()
    {
        var result = NameParser.NormalizeCase("SMITH-JONES");

        Assert.AreEqual("Smith-Jones", result);
    }

    [TestMethod]
    public void NormalizeCase_HyphenatedWithMc_HandlesComplexCase()
    {
        var result = NameParser.NormalizeCase("MCDONALD-JONES");

        Assert.AreEqual("McDonald-Jones", result);
    }

    [TestMethod]
    public void NormalizeCase_NullOrEmpty_ReturnsEmpty()
    {
        Assert.AreEqual("", NameParser.NormalizeCase(null));
        Assert.AreEqual("", NameParser.NormalizeCase(""));
        Assert.AreEqual("", NameParser.NormalizeCase("   "));
    }

    [TestMethod]
    public void NormalizeCase_MultipleWords_NormalizesEach()
    {
        var result = NameParser.NormalizeCase("MARY JANE WATSON");

        Assert.AreEqual("Mary Jane Watson", result);
    }

    #endregion

    #region ExtractProviderLastName Tests

    [TestMethod]
    public void ExtractProviderLastName_DrPrefix_ExtractsLastName()
    {
        Assert.AreEqual("Smith", NameParser.ExtractProviderLastName("Dr Smith"));
        Assert.AreEqual("Smith", NameParser.ExtractProviderLastName("Dr. Smith"));
        Assert.AreEqual("Smith", NameParser.ExtractProviderLastName("DR SMITH"));
    }

    [TestMethod]
    public void ExtractProviderLastName_NpPrefix_ExtractsLastName()
    {
        Assert.AreEqual("Jones", NameParser.ExtractProviderLastName("NP Jones"));
        Assert.AreEqual("Jones", NameParser.ExtractProviderLastName("NP. Jones"));
    }

    [TestMethod]
    public void ExtractProviderLastName_DoctorPrefix_ExtractsLastName()
    {
        Assert.AreEqual("Williams", NameParser.ExtractProviderLastName("Doctor Williams"));
    }

    [TestMethod]
    public void ExtractProviderLastName_MdSuffix_ExtractsLastName()
    {
        Assert.AreEqual("Smith", NameParser.ExtractProviderLastName("Smith, MD"));
        Assert.AreEqual("Smith", NameParser.ExtractProviderLastName("Smith MD"));
        Assert.AreEqual("Smith", NameParser.ExtractProviderLastName("John Smith MD"));
        Assert.AreEqual("Smith", NameParser.ExtractProviderLastName("John Smith, MD"));
    }

    [TestMethod]
    public void ExtractProviderLastName_DoSuffix_ExtractsLastName()
    {
        Assert.AreEqual("Jones", NameParser.ExtractProviderLastName("Jones DO"));
        Assert.AreEqual("Jones", NameParser.ExtractProviderLastName("Jones, DO"));
    }

    [TestMethod]
    public void ExtractProviderLastName_NpSuffix_ExtractsLastName()
    {
        Assert.AreEqual("Brown", NameParser.ExtractProviderLastName("Brown NP"));
        Assert.AreEqual("Brown", NameParser.ExtractProviderLastName("Sarah Brown, NP"));
    }

    [TestMethod]
    public void ExtractProviderLastName_PlainName_ReturnsAsIs()
    {
        Assert.AreEqual("Smith", NameParser.ExtractProviderLastName("Smith"));
    }

    [TestMethod]
    public void ExtractProviderLastName_MultipleWordsNoPattern_TakesLastWord()
    {
        Assert.AreEqual("Johnson", NameParser.ExtractProviderLastName("Robert Johnson"));
    }

    [TestMethod]
    public void ExtractProviderLastName_NullOrEmpty_ReturnsNull()
    {
        Assert.IsNull(NameParser.ExtractProviderLastName(null));
        Assert.IsNull(NameParser.ExtractProviderLastName(""));
        Assert.IsNull(NameParser.ExtractProviderLastName("   "));
    }

    [TestMethod]
    public void ExtractProviderLastName_SpecialNames_NormalizesCase()
    {
        Assert.AreEqual("McDonald", NameParser.ExtractProviderLastName("Dr MCDONALD"));
        Assert.AreEqual("O'Brien", NameParser.ExtractProviderLastName("O'BRIEN MD"));
    }

    #endregion

    #region ParseLocation Tests

    [TestMethod]
    public void ParseLocation_ValidFormat_SplitsRoomAndBed()
    {
        var (room, bed) = NameParser.ParseLocation("123A");

        Assert.AreEqual("123", room);
        Assert.AreEqual("A", bed);
    }

    [TestMethod]
    public void ParseLocation_MultiDigitRoom_SplitsCorrectly()
    {
        var (room, bed) = NameParser.ParseLocation("1001B");

        Assert.AreEqual("1001", room);
        Assert.AreEqual("B", bed);
    }

    [TestMethod]
    public void ParseLocation_MultipleBedLetters_SplitsCorrectly()
    {
        var (room, bed) = NameParser.ParseLocation("205AB");

        Assert.AreEqual("205", room);
        Assert.AreEqual("AB", bed);
    }

    [TestMethod]
    public void ParseLocation_NullOrEmpty_ReturnsNulls()
    {
        var (room1, bed1) = NameParser.ParseLocation(null);
        var (room2, bed2) = NameParser.ParseLocation("");
        var (room3, bed3) = NameParser.ParseLocation("   ");

        Assert.IsNull(room1);
        Assert.IsNull(bed1);
        Assert.IsNull(room2);
        Assert.IsNull(bed2);
        Assert.IsNull(room3);
        Assert.IsNull(bed3);
    }

    [TestMethod]
    public void ParseLocation_InvalidFormat_ReturnsNulls()
    {
        // Only letters
        var (room1, bed1) = NameParser.ParseLocation("ABC");
        Assert.IsNull(room1);
        Assert.IsNull(bed1);

        // Only numbers
        var (room2, bed2) = NameParser.ParseLocation("123");
        Assert.IsNull(room2);
        Assert.IsNull(bed2);

        // Letters before numbers
        var (room3, bed3) = NameParser.ParseLocation("A123");
        Assert.IsNull(room3);
        Assert.IsNull(bed3);
    }

    #endregion
}
