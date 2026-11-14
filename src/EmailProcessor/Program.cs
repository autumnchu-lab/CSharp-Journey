namespace EmailProcessor
{
    internal class Program
    {
        public class Customer
        {
            public string Name { get; set; }
            public string? Email { get; set; }
        }

        public static string? GetEmailDomain(Customer customer)
        {
            if (customer == null)
            {
                throw new ArgumentNullException(nameof(customer), "Customer cannot be null.");
            }

            if (customer.Email == null)
            {
                return null;
            }

            string[] emailParts = customer.Email.Split('@');
            if (emailParts.Length != 2 || string.IsNullOrEmpty(emailParts[0]) || string.IsNullOrEmpty(emailParts[1]))
            {
                return null;
            }

            string trimmedDomainPart = emailParts[1].Trim();
            if (string.IsNullOrEmpty(trimmedDomainPart))
            {
                return null;
            }

            return trimmedDomainPart;
        }

        public static string? DomainType(string? domain)
        {
            if (domain == null)
            {
                return null;
            }

            string trimmedDomain = domain.Trim();

            if (trimmedDomain == "")
            {
                return "無網域";
            }

            string lowerDomain = trimmedDomain.ToLower();

            switch (lowerDomain)
            {
                case "gmail.com":
                case "yahoo.com.tw":
                case "hotmail.com":
                    return "個人信箱";
                default:
                    return "公司信箱";
            }
        }
        static void Main(string[] args)
        {
            var customers = new List<Customer> {
                new Customer { Email = null },
                new Customer { Email = "" },
                new Customer { Email = " " },
                new Customer { Email = "user@ gmail.com" },
                new Customer { Email = "user@GMAIL.COM" },
                new Customer { Email = "user@gmail.com" },
                new Customer { Email = "user@yahoo.com.tw" },
                new Customer { Email = "user@hotmail.com" },
                new Customer { Email = "user@company.com" },
                new Customer { Email = "user@ " }
            };

            int success = 0;
            int invalidInput = 0;
            int nullCustomer = 0;
            int undefinedDomainType = 0;

            List<int> wrongInputPosition = new List<int>();
            for (int i = 0; i < customers.Count; i++)
            {
                int counter;
                counter = i + 1;
                try
                {
                    string? domain = GetEmailDomain(customers[i]);
                    if (domain == null)
                    {
                        Console.WriteLine($"第 {counter} 筆：無資料或Email 格式不正確，請重新輸入");
                        invalidInput++;
                        wrongInputPosition.Add(counter);
                    }
                    else
                    {
                        string? domainType = DomainType(domain);
                        if (domainType == null)
                        {
                            Console.WriteLine($"第 {counter} 筆：Email domain: {domain}, Domain Type: 無法判斷");
                            undefinedDomainType++;
                        }
                        else
                        {
                            Console.WriteLine($"第 {counter} 筆：Email domain: {domain}, Domain Type: {domainType}");
                            success++;
                        }
                    }
                }
                catch (ArgumentNullException ex)
                {
                    Console.WriteLine($"第 {counter} 筆：資料有問題");
                    nullCustomer++;
                }
            }
            string wrongInputResult = wrongInputPosition.Count > 0 ? string.Join(",", wrongInputPosition) : "無";
            Console.WriteLine($"成功: {success}, 無效輸入: {invalidInput}, 無客戶: {nullCustomer}, 客戶數: {customers.Count}, 格式不正確的筆數編號: {wrongInputResult}, 無法判斷的Domain Type: {undefinedDomainType}");

            //測試DomainType
            var domains = new List<string?> {
                null,
                "",
                " ",
                " gmail.com",
                "GMAIL.COM",
                "gmail.com",
                "yahoo.com.tw",
                "hotmail.com",
                "company.com"
            };

            for (int i = 0; i < domains.Count; i++)
            {
                string? domainType = DomainType(domains[i]);
                string? input;
                string? output = domainType == null ? "null" : domainType;
                int counter;
                counter = i + 1;
                if (domains[i] == null)
                {
                    input = "null";
                }
                else if (domains[i] == "")
                {
                    input = "無資料";
                }
                else
                {
                    input = domains[i];
                }
                Console.WriteLine($"第 {counter} 筆 輸入值: {input}, 回傳值: {output}");
            }


            var amounts = new List<decimal> { -5m, -2m, -9m };
            decimal totalAmount = 0;
            decimal maxAmount = 0;

            if (amounts.Count > 0)
            {
                maxAmount = amounts[0];
            }
            for (int i = 0; i < amounts.Count; i++)
            {
                totalAmount += amounts[i];
                if (amounts[i] > maxAmount)
                {
                    maxAmount = amounts[i];
                }
            }

            string amountResult = amounts.Count > 0 ? maxAmount.ToString() : "無";
            Console.WriteLine($"總金額: {totalAmount}, 最大金額: {amountResult}");
        }
    }
}
