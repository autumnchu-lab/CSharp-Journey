# Week 1 C# 基礎筆記（Session 1–4）

> 範圍：Session 1–4。四個 Session 用**同一個 Console 專案**（`ConsoleApp2`，.NET 8，保留 `Main`）逐步演進。
> 註記：Session 1–3 的內容整理自進度紀錄與目前程式碼；Session 4 的內容最完整。請你對照自己當時的理解，修正不準確的地方。

---

## 0. 這個專案在做什麼

**情境**：有一份客戶清單，每位客戶有 Email。要從 Email 切出網域，判斷是個人信箱還是公司信箱，並統計處理結果。

**資料流**

```
customers[i]（Customer）
   │
   ▼
GetEmailDomain(customer)     → string?   切出網域；Email 不能用就回傳 null
   │
   ▼
DomainType(domain)           → string?   個人信箱 / 公司信箱 / 無網域 / null
   │
   ▼
Main                         印出結果、更新計數器
```

**各層責任**

| 方法 | 負責 |
|---|---|
| `GetEmailDomain` | 從 Email 切出網域，並判斷這個 Email 能不能切出可用的網域 |
| `DomainType` | 拿到一個網域，判斷它是個人、公司，還是沒有網域 |
| `Main` | 呼叫上面兩個、印出、計數 |

**演進**

| Session | 主題 | 程式碼上的變化 |
|---|---|---|
| 1 | null 與例外 | `Customer`、`ArgumentNullException`、`try/catch` |
| 2 | 變數、迴圈、`List` | `List<Customer>`、`for`、計數器、`decimal` 加總 |
| 3 | `if` / `switch`、簽章契約 | `DomainType` 的 `switch`、`string?` 的設計 |
| 4 | 契約落實、測試設計 | null 守門、`Trim`、case 堆疊、測試表、第 4 個計數器 |

---

## Session 1：null 與例外

### null 是什麼
- `null` 代表「沒有值」，不是 `""`（空字串），也不是 `0`。
- 對 null 呼叫方法或存取成員，執行時會丟 `NullReferenceException`。

### 例外與 try/catch
```csharp
try
{
    string? domain = GetEmailDomain(customers[i]);
    // ...
}
catch (ArgumentNullException)
{
    Console.WriteLine($"第 {counter} 筆：資料有問題");
    nullCustomer++;
}
```
- `try` 放可能出問題的程式；`catch (例外型別)` 只接指定型別的例外。
- `throw new ArgumentNullException(nameof(customer), "...")`：方法**主動**宣告「這個參數不能是 null」。
- `nameof(customer)` 取得參數名稱，改名時不會漏改。

### 兩種處理壞資料的方式（Session 3、4 延伸）
| 方式 | 適用 |
|---|---|
| 丟例外 | **不該發生**的情況（程式錯誤）。例：客戶物件本身是 null |
| 驗證後回傳 | **預期會出現**的壞資料。例：Email 欄位沒填、格式不對 |

迴圈處理一批資料時，預期的壞資料如果丟例外，就得靠 `try/catch` 才能繼續；回傳 null 則是一般的 `if` 判斷，流程自然往下走。

### 例外名稱（容易混淆）
| 情境 | 例外 |
|---|---|
| 陣列 `arr[5]` 越界 | `IndexOutOfRangeException` |
| `List<T>` 的 `list[5]` 越界 | `ArgumentOutOfRangeException` |
| 對 null 呼叫成員 | `NullReferenceException` |
| 參數不該是 null | `ArgumentNullException` |

> 「位置不存在」不等於「值是 null」。`"".Split('@')` 得到長度 1 的陣列，裡面是 `""`，位置 1 不存在。

---

## Session 2：變數、迴圈、List

### 變數與型別
- `int` 計數、`string` 文字、`decimal` 金額（金額用 `decimal`，不用 `double`）、`bool` 真假。
- `string?`：可能是 null 的字串。

### List 與 for
```csharp
var customers = new List<Customer> { ... };

for (int i = 0; i < customers.Count; i++)
{
    int counter = i + 1;      // i 從 0 開始，顯示給人看用 i + 1
    // customers[i] ...
}
```
- `List` 用 `.Count`，陣列用 `.Length`。
- 迴圈變數 `i` 是索引（位置），不是第幾筆。

### 計數器模式
```csharp
int success = 0;
int invalidInput = 0;
int nullCustomer = 0;
List<int> wrongInputPosition = new List<int>();
```
- 迴圈裡每筆資料依結果更新對應的計數器；迴圈結束後一次輸出。
- 三元運算子處理「沒資料」的顯示：
```csharp
string wrongInputResult = wrongInputPosition.Count > 0
    ? string.Join(",", wrongInputPosition)
    : "無";
```

### 找最大值的初始值陷阱
```csharp
var amounts = new List<decimal> { -5m, -2m, -9m };
decimal maxAmount = 0;
for (...) { if (amounts[i] > maxAmount) maxAmount = amounts[i]; }
```
- 這段程式的輸出是 `最大金額: 0`，但 -5、-2、-9 的最大值應該是 -2。
- 原因：`maxAmount` 初始值是 0，比所有元素都大，`if` 永遠不成立。
- 這個輸出是 bug，不是正確結果。程式碼裡有一段被註解掉的 `maxAmount = amounts[0]`，就是修正方向（用第一個元素當初始值，並先檢查 `Count > 0`）。
- **請確認**：這段是你刻意保留成反例，還是還沒修。

---

## Session 3：if / switch、簽章契約

### if 與 switch 的選擇
- `if`：條件是範圍或複雜的布林判斷。
- `switch`：拿**同一個值**去比對多個**固定值**。

### switch 的執行模型
- **比對一次**，找到符合的標籤後，**從標籤下方的語句開始執行**，直到 `return` 或 `break`。
- **標籤不是語句**：`case "gmail.com":` 只是標示執行起點，本身不執行任何東西。
- 沒有符合的標籤，走 `default`。

### 簽章是契約
```csharp
public static string? GetEmailDomain(Customer customer)
public static string? DomainType(string? domain)
```

| 寫法 | 意義 |
|---|---|
| `string` | 承諾不是 null |
| `string?` | 可能是 null，呼叫端要處理 |

- **契約 B**：`DomainType` 收到 null 回傳 null（預期的壞資料，用驗證加回傳，不丟例外）。
- **契約層 vs 執行層**：簽章寫 `string` 卻傳進 null，編譯器**只給警告**（CS8602 一類），不是錯誤。程式能跑，執行時才可能出事。契約由你守，編譯器只提醒。

---

## Session 4：落實契約、字串處理、測試設計

### 4.1 `string` 不可變
- `ToLower()`、`Trim()`、`Replace()` 都是**回傳新字串**，原字串不動。
```csharp
domain.ToLower();                 // ✗ 結果被丟掉
string lower = domain.ToLower();  // ✓ 用變數接住
```
- 實例：`DomainType` 裡，`domain` 保持原樣（`Gmail.com`），`lowerDomain` 才是小寫版。所以 Main 印出原樣，`switch` 比的是小寫。

### 4.2 `Trim()` 只處理頭尾
- 只去掉整個字串**最前面與最後面**的空白，中間的不動。
- `"user@ gmail.com"` 整個字串的頭尾沒有空白，`Trim()` 沒效果。
- 要對 `Split` 之後的 `emailParts[1]`（`" gmail.com"`）做 `Trim()`，空白才落在那個字串的開頭。
- 規則：`Trim()` 作用在哪個字串，決定它有沒有用。

### 4.3 case 堆疊
```csharp
switch (lowerDomain)
{
    case "gmail.com":
    case "yahoo.com.tw":
    case "hotmail.com":
        return "個人信箱";
    default:
        return "公司信箱";
}
```
- 所有 `case` 都在同一層，不是巢狀。
- **沒有語句的標籤**，執行會落到下一個標籤的語句。
- 有語句卻沒有 `break` / `return` 的 case，C# 編譯器會報錯，不會默默穿過去。
- 這裡 `return` 直接結束方法，不需要 `break;`。

### 4.4 兩種不同的 null
```csharp
string? domain = GetEmailDomain(customers[i]);
if (domain == null)               // 外層：沒有網域可用
{ ... invalidInput++ ... }
else
{
    string? domainType = DomainType(domain);
    if (domainType == null)       // 內層：有網域，但沒有判斷結果
    { ... undefinedDomainType++ ... }
    else
    { ... success++ ... }
}
```

| | 哪個變數是 null | 當時有沒有網域 | Main 印出 |
|---|---|---|---|
| 外層 | `domain` | 沒有 | 無資料或 Email 格式不正確 |
| 內層 | `domainType` | 有 | 無法判斷 |

- 內層分支在目前資料下**不會被觸發**（`DomainType` 只有收到 null 才回傳 null，而外層已擋掉）。它是防禦性程式碼，從未被實測過。
- 字串插值 `$"{x}"` 遇到 null 不丟例外，會變成**空字串**。`null` 和 `""` 印出來都是空白，測試時要自己讓它們看得出差別。

### 4.5 null 與 `""` 是不同狀況
- `null`：沒有值。`""`：有一個值，但是空的。
- `DomainType("")` 回傳 `"無網域"`（需求決定），不回傳 null。
- null 只說「有問題」，不說原因。要知道原因，要回傳有意義的字串（或用 `TryXxx` 模式，Session 5 會教）。

### 4.6 短路求值與索引安全
```csharp
if (emailParts.Length != 2 || string.IsNullOrEmpty(emailParts[0]) || string.IsNullOrEmpty(emailParts[1]))
    return null;
```
- `||` 左邊結果已是 `true`，整個條件已確定，**右邊不會執行**。
- 所以 `Length` 檢查放最前面，後面的 `[0]`、`[1]` 才安全。
- 規則：**用 `[n]` 之前，先確認第 n 個位置存在。**
- 實例（Debug）：曾把 `emailParts[1].Trim()` 放在 `Length` 檢查之前，輸入 `Email = ""` 時丟 `IndexOutOfRangeException`。

### 4.7 處理順序：先轉換，後檢查

`DomainType`：
```
null 檢查 → Trim → 空字串檢查 → ToLower → switch
```

`GetEmailDomain`：
```
customer 檢查 → Email null 檢查 → Split → 第一道檢查（位置存在、前段不空）
→ Trim → 第二道檢查（Trim 後是否為空）→ 回傳
```
- 第一道檢查要在 `Trim` 之前（確保位置 1 存在）。
- 第二道檢查要在 `Trim` 之後（`" "` 在 Trim 前通過 `IsNullOrEmpty`，Trim 後才變成空）。
- 轉換之後的值要重新檢查；檢查和最後的 `return` 要用**同一個變數**。

### 4.8 層級責任
- **資料在哪裡產生，就在哪裡判斷能不能用。** 空白網域由 `GetEmailDomain` 擋掉，後面拿到的就是乾淨的資料。
- `public` 方法的簽章是對外承諾，不能假設呼叫端一定先經過別的方法，所以 `DomainType` 自己也要處理 null 與空白。
- 不要讓 Main 去比對某個特定字串（例如 `"無網域"`）來猜壞資料，字串一改就悄悄壞掉。

### 4.9 計數器的不變式
> 每一筆資料必須剛好落在一個計數器。
> 成功 + 無效輸入 + 無客戶 + 無法判斷 = 客戶數。

- 加總成立，只代表「每一筆都落在某一類」，**不代表落對了類**。
- 實例：修 `Trim` 時，`"user@ "` 從無效輸入退回成功，加總仍成立，分類卻是錯的。
- 規則：修一個問題後，**重跑全部測試資料**，確認沒有弄壞別的。

### 4.10 測試設計
- 測試表欄位：輸入、期望、實際、結果。
- **期望欄只看規則，不看 code。** 不確定的規則標「？」，代表需求還沒決定。「待定」的列不能標通過。
- **測要測的那一層**：從 Main 流程測 `DomainType`，`null` 和 `""` 會先被 `GetEmailDomain` 擋掉，量不到。要把測試值**直接丟給** `DomainType`，而且**每一筆都呼叫**，不能先用 `if` 攔截。
- 顯示用的文字不要和資料混在一起（例如把 `""` 顯示成「無資料」，畫面上就看不出它是輸入）。

**`DomainType` 測試表（最終）**

| 輸入 | 期望 | 結果 |
|---|---|---|
| `null` | `null` | 通過 |
| `""` | 無網域 | 通過 |
| `" "` | 無網域 | 通過 |
| `" gmail.com"` | 個人信箱 | 通過 |
| `"GMAIL.COM"` | 個人信箱 | 通過 |
| `"gmail.com"` | 個人信箱 | 通過 |
| `"yahoo.com.tw"` | 個人信箱 | 通過 |
| `"hotmail.com"` | 個人信箱 | 通過 |
| `"company.com"` | 公司信箱 | 通過 |

**`GetEmailDomain` 邊界**

| Email | 回傳 | 結果 |
|---|---|---|
| `""` | `null` | 通過 |
| `"user@ gmail.com"` | `"gmail.com"` | 通過 |
| `"user@ "` | `null` | 通過 |

### 4.11 Debug 流程（這次用到的）
1. 重現：用具體輸入（`Email = ""`）。
2. 定位：哪一行出錯。
3. 看例外名稱與訊息。
4. 追蹤資料：`Split` 後長度、位置 1 是否存在。
5. 找根因：執行順序。
6. 修改後重跑全部測試資料。

---

## 概念總表

| 概念 | 一句話 | 出現 |
|---|---|---|
| null vs `""` | 沒有值 vs 有值但是空的 | S1、S4 |
| 例外 vs 驗證回傳 | 不該發生才丟；預期壞資料用回傳 | S1、S3 |
| `string?` 契約 | 簽章是承諾，編譯器只警告 | S3、S4 |
| `for` + 計數器 | 一次走一筆，迴圈後統計 | S2 |
| `switch` 模型 | 比對一次，從標籤下方開始執行 | S3、S4 |
| `string` 不可變 | 方法回傳新字串，要自己接住 | S4 |
| `Trim` | 只去頭尾，中間不動 | S4 |
| 短路求值 | `||` 左邊為真，右邊不執行 | S4 |
| 層級責任 | 資料在哪產生，就在哪判斷 | S4 |
| 不變式 | 每筆資料剛好落在一個計數器 | S4 |
| 測試表 | 期望只看規則；測要測的那一層 | S4 |

---

## 自我檢核：反覆出錯的地方

| 模式 | 提醒自己 |
|---|---|
| 陣列與 `List` 例外名稱混淆 | 陣列 `IndexOutOfRangeException`；`List` `ArgumentOutOfRangeException` |
| 把「位置不存在」當成 null | 位置不存在會丟例外，值是 null 是另一回事 |
| 逐步追蹤時合併步驟 | 每一步寫出值，不要只寫結論 |
| 把想要的行為當成 code 會做的行為 | 預測前先讀 code 實際做什麼 |
| 層級責任放錯層 | 先問：資料在哪裡產生？ |
| 結論對、貼的輸出沒量到想量的那一層 | 測之前先確認呼叫路徑不會被前面的步驟攔掉 |
| 參數與回傳值用詞混用 | 傳進去的是參數，傳出來的是回傳值（`IsNullOrEmpty(" ")` 的回傳值是 `false`） |
| 修一個問題時弄壞另一個 | 改完重跑全部測試資料 |

---

## 尚未完成（Session 5 之後）

- `while` / `break` / `continue`
- `TryXxx` 模式 vs 回傳 null 的取捨
- `decimal?`
- `async` / `await` 概述
- 自己從零設計新的邊界輸入
