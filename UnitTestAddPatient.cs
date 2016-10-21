using NUnit.Framework;
using UnitTestAddPatient.ServiceAddPatient;
using System.ServiceModel;
using System;
using System.Collections.Generic;

namespace UnitTestAddPatient
{
    [TestFixture]
    public class UnitTestAddPatient
    {

        PixServiceClient service;
        PatientDto patient;
        String guid = "8CDE415D-FAB7-4809-AA37-8CDD70B1B46C";
        String idLPU = "1.2.643.5.1.13.3.25.78.118";

        [OneTimeSetUp]
        public void Init()
        {
            this.service = new PixServiceClient();
            this.patient = new PatientDto();
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            this.service.Close();
        }
        
        [Test, Description("1. Пустые или некорректные идентификаторы")]
        public void TestIncorrectId()
        {
            Assert.Throws(typeof(FaultException<RequestFault>),
                delegate { this.service.AddPatient(this.guid, "", this.patient); });
            Assert.Throws(typeof(FaultException<RequestFault>),
                delegate { this.service.AddPatient(this.guid, "dfghdfg", this.patient); });
            Assert.Throws(typeof(FaultException<RequestFault>),
                delegate { this.service.AddPatient("", this.idLPU, this.patient); });
            Assert.Throws(typeof(FaultException<RequestFault>),
                delegate { this.service.AddPatient("dfgdfg", this.idLPU, this.patient); });
            Assert.Throws(typeof(FaultException<RequestFault>),
                delegate { this.service.AddPatient("", "", this.patient); });
            Assert.Throws(typeof(FaultException<RequestFault>),
                delegate { this.service.AddPatient("dgjelk", "dgslkg", this.patient); });
        }

        [Test, Description("2. Добавление пацента")]
        public void TestCorrectPatient()
        {
            this.patient = SetCorrectPatient();
            this.AddPatient();
            PatientDto[] patient = this.GetPatient();            
            Assert.That(this.patient, Is.EqualTo(patient[0]).Using(new PatientComparer()));            
        }

        [Test, Description("3. Дубли")]
        public void TestDuplicatePatients()
        {
            this.patient = SetCorrectPatient();
            this.AddPatient();
            Assert.Throws(typeof(FaultException<RequestFault>),
                delegate { this.AddPatient(); });

            this.patient = SetCorrectPatient();
            this.AddPatient();
            this.patient.IdBloodType = 1;
            Assert.Throws(typeof(FaultException<RequestFault>),
                delegate { this.AddPatient(); });

            this.patient = SetCorrectPatient();
            this.AddPatient();
            this.patient.IdPatientMIS = Guid.NewGuid().ToString();
            Assert.Throws(typeof(FaultException<RequestFault>),
                delegate { this.AddPatient(); });
        }

        [Test, Description("4. Логика дат")]
        public void TestDates()
        {
            this.patient = SetCorrectPatient();
            this.patient.BirthDate = new DateTime().AddDays(1);
            Assert.Throws(typeof(FaultException<RequestFault>),
                delegate { this.AddPatient(); });

            this.patient = SetCorrectPatient();
            this.patient.DeathTime = this.patient.BirthDate.AddDays(-1);
            Assert.Throws(typeof(FaultException<RequestFault>),
                delegate { this.AddPatient(); });
        }

        [Test, Description("5. Соответствие справочникам")]
        public void TestBooks()
        {
            this.patient = SetCorrectPatient();
            this.patient.Sex = 0;
            Assert.Throws(typeof(FaultException),
                delegate { this.AddPatient(); });            
        }

        [Test, Description("6. Маски значений")]
        public void TestMasks()
        {
            this.patient = SetCorrectPatient();

            DocumentDto snils = new DocumentDto();
            snils.IdDocumentType = 223;
            snils.DocN = "некорректныйснилс";
            snils.ProviderName = "ПФР";
            DocumentDto enp = new DocumentDto();
            enp.IdDocumentType = 228;
            enp.DocN = "некорректныйенп";
            enp.ProviderName = "СМО";
            this.patient.Documents = new DocumentDto[] { snils, enp };


            Assert.Throws(typeof(FaultException<RequestFault>),
                delegate { this.AddPatient(); });
        }

        [Test, Description("7. Параметры адреса")]
        public void TestAddresses()
        {
            this.patient = SetCorrectPatient();

            AddressDto address = new AddressDto();
            address.IdAddressType = 1;
            address.StringAddress = "Новый некорректный адрес";
            address.City = "некорректныйкладргорода";
            address.Street = "некорректныйкладрулицы";
            address.GeoData = "некорректныекоординаты";


            this.patient.Addresses = new AddressDto[] { address };
            Assert.Throws(typeof(FaultException<RequestFault>),
                delegate { this.AddPatient(); });
        }

        public void AddPatient()
        {
          this.service.AddPatient(this.guid, this.idLPU, this.patient);
        }

        public PatientDto[] GetPatient()
        {            
            return this.service.GetPatient(this.guid, this.idLPU, this.patient, SourceType.Reg);
        }

        public static PatientDto SetCorrectPatient()
        {
            PatientDto patient = new PatientDto();
            patient.BirthDate = new DateTime(1984, 4, 30);
            patient.FamilyName = "Локтионов";
            patient.GivenName = "Артем";
            patient.IdPatientMIS = Guid.NewGuid().ToString();
            patient.Sex = 1;
            return patient;
        }
    }

    class PatientComparer : IEqualityComparer<PatientDto>
    {
        public bool Equals(PatientDto x, PatientDto y)
        {
            return (
                x.BirthDate == y.BirthDate
                & x.FamilyName == y.FamilyName
                & x.GivenName == y.GivenName
                & x.IdPatientMIS == y.IdPatientMIS
                & x.Sex == y.Sex
                );
        }

        public int GetHashCode(PatientDto obj)
        {
            throw new NotImplementedException();
        }
    }
}
