using System;
using System.Collections.Generic;

namespace AiLaTrieuPhu
{
    // Bốn lựa chọn chủ đề cho giao diện.
    // Mỗi ván chỉ lấy câu thuộc một chủ đề đã chọn.
    public enum QuestionTopic
    {
        Programming,              // Lập trình C#.
        Networking,               // Mạng máy tính và Internet.
        Databases,                // Cơ sở dữ liệu và SQL.
        HardwareOperatingSystems  // Phần cứng và hệ điều hành.
    }

    // Quản lý ngân hàng câu hỏi và chuẩn bị câu hỏi cho mỗi ván.
    // Không chứa luật tính tiền hoặc thành phần giao diện.
    public class QuestionBank
    {
        // Số câu cần lấy từ từng tầng trong một ván.
        public const int EasyQuestionsPerGame = 3;
        public const int MediumQuestionsPerGame = 4;
        public const int HardQuestionsPerGame = 3;

        // Tổng số câu của một ván theo SPEC v2.
        public const int QuestionsPerGame =
            EasyQuestionsPerGame +
            MediumQuestionsPerGame +
            HardQuestionsPerGame;

        // Số câu tối thiểu phải có trong MỖI chủ đề.
        public const int MinimumEasyQuestions = 9;
        public const int MinimumMediumQuestions = 12;
        public const int MinimumHardQuestions = 9;

        // Dữ liệu gốc được giữ riêng.
        // Khi chọn và trộn, luôn tạo đối tượng câu hỏi mới.
        private readonly List<Question> _questions;

        // Dùng chung một bộ tạo số ngẫu nhiên trong ngân hàng.
        // Không tạo lại bộ này cho từng lần trộn.
        private readonly Random _random;

        // Chủ đề cố định của ngân hàng này.
        // Tạo ngân hàng mới khi người chơi chọn chủ đề khác.
        public QuestionTopic SelectedTopic { get; }

        // Cung cấp danh sách chỉ đọc của chủ đề đã chọn.
        // Bên nhận không thể thêm hoặc xóa câu trong ngân hàng.
        public IReadOnlyList<Question> AllQuestions
        {
            get
            {
                return _questions.AsReadOnly();
            }
        }

        // Giữ tương thích với cách gọi cũ.
        // Nếu không truyền chủ đề, mặc định chơi Lập trình C#.
        public QuestionBank()
            : this(QuestionTopic.Programming, new Random())
        {
        }

        // Giữ cách khởi tạo cũ của TestRunner.
        // Số khởi đầu cố định giúp tái hiện các lượt chọn câu.
        public QuestionBank(int randomSeed)
            : this(QuestionTopic.Programming, new Random(randomSeed))
        {
        }

        // Giao diện dùng cách này để chọn chủ đề cho toàn bộ ván.
        public QuestionBank(QuestionTopic selectedTopic)
            : this(selectedTopic, new Random())
        {
        }

        // Cho phép kiểm thử riêng từng chủ đề với số khởi đầu cố định.
        public QuestionBank(QuestionTopic selectedTopic, int randomSeed)
            : this(selectedTopic, new Random(randomSeed))
        {
        }

        // Chỉ nạp dữ liệu của chủ đề được chọn.
        // Chủ đề không hợp lệ sẽ bị từ chối, không tự đổi sang chủ đề khác.
        private QuestionBank(QuestionTopic selectedTopic, Random random)
        {
            _random = random;
            SelectedTopic = selectedTopic;
            _questions = CreateDefaultQuestions(selectedTopic);

            ValidateBank();
        }

        // Trả về bốn lựa chọn để giao diện tạo danh sách chủ đề.
        public static IReadOnlyList<QuestionTopic> GetAvailableTopics()
        {
            return Array.AsReadOnly(new QuestionTopic[]
            {
                QuestionTopic.Programming,
                QuestionTopic.Networking,
                QuestionTopic.Databases,
                QuestionTopic.HardwareOperatingSystems
            });
        }

        // Chuyển giá trị chủ đề thành tên tiếng Việt để hiển thị.
        public static string GetTopicName(QuestionTopic topic)
        {
            switch (topic)
            {
                case QuestionTopic.Programming:
                    return "Lập trình C#";

                case QuestionTopic.Networking:
                    return "Mạng máy tính và Internet";

                case QuestionTopic.Databases:
                    return "Cơ sở dữ liệu và SQL";

                case QuestionTopic.HardwareOperatingSystems:
                    return "Phần cứng và hệ điều hành";

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(topic),
                        "Chủ đề không hợp lệ.");
            }
        }

        // Kiểm tra các điều kiện thuộc phạm vi toàn ngân hàng.
        // Điều kiện bên trong từng câu đã được Question kiểm tra.
        public void ValidateBank()
        {
            HashSet<string> usedIds =
                new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            int easyCount = 0;
            int mediumCount = 0;
            int hardCount = 0;

            foreach (Question question in _questions)
            {
                // Không cho phép phần tử thiếu đối tượng câu hỏi.
                if (question == null)
                {
                    throw new InvalidOperationException(
                        "Ngân hàng có phần tử không chứa câu hỏi.");
                }

                // Mỗi mã chỉ được xuất hiện một lần.
                // Không phân biệt chữ hoa, chữ thường khi xét mã.
                if (!usedIds.Add(question.Id))
                {
                    throw new InvalidOperationException(
                        "Ngân hàng có mã câu hỏi bị trùng: " +
                        question.Id);
                }

                // Đếm số câu trong từng tầng.
                switch (question.Difficulty)
                {
                    case QuestionDifficulty.Easy:
                        easyCount++;
                        break;

                    case QuestionDifficulty.Medium:
                        mediumCount++;
                        break;

                    case QuestionDifficulty.Hard:
                        hardCount++;
                        break;

                    default:
                        throw new InvalidOperationException(
                            "Câu hỏi có tầng độ khó không hợp lệ: " +
                            question.Id);
                }
            }

            // Kiểm tra số lượng tối thiểu theo SPEC v2.
            if (easyCount < MinimumEasyQuestions)
            {
                throw new InvalidOperationException(
                    "Ngân hàng cần ít nhất " +
                    MinimumEasyQuestions + " câu dễ.");
            }

            if (mediumCount < MinimumMediumQuestions)
            {
                throw new InvalidOperationException(
                    "Ngân hàng cần ít nhất " +
                    MinimumMediumQuestions + " câu trung bình.");
            }

            if (hardCount < MinimumHardQuestions)
            {
                throw new InvalidOperationException(
                    "Ngân hàng cần ít nhất " +
                    MinimumHardQuestions + " câu khó.");
            }
        }

        // Tạo đúng 10 câu cho một ván.
        //
        // excludedQuestionIds là các mã cần tránh.
        // GameRules sẽ truyền toàn bộ mã của ván liền trước vào đây.
        // Ván đầu tiên có thể không truyền danh sách này.
        //
        // Thứ tự kết quả:
        // 3 câu dễ, tiếp theo 4 câu trung bình, cuối cùng 3 câu khó.
        public IReadOnlyList<Question> CreateGameQuestions(
            IEnumerable<string> excludedQuestionIds = null)
        {
            // Dùng tập hợp để kiểm tra mã cần tránh nhanh và rõ ràng.
            HashSet<string> excludedIds =
                new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (excludedQuestionIds != null)
            {
                foreach (string id in excludedQuestionIds)
                {
                    if (string.IsNullOrWhiteSpace(id))
                    {
                        throw new ArgumentException(
                            "Danh sách mã cần tránh có mã trống.",
                            nameof(excludedQuestionIds));
                    }

                    excludedIds.Add(id.Trim());
                }
            }

            // Lọc riêng từng tầng và bỏ các câu thuộc ván trước.
            List<Question> easyPool = GetAvailableQuestions(
                QuestionDifficulty.Easy,
                excludedIds);

            List<Question> mediumPool = GetAvailableQuestions(
                QuestionDifficulty.Medium,
                excludedIds);

            List<Question> hardPool = GetAvailableQuestions(
                QuestionDifficulty.Hard,
                excludedIds);

            // Kiểm tra đủ câu trước khi bắt đầu chọn.
            // Nếu thiếu thì báo lỗi, không tự dùng lại câu bị loại.
            EnsureEnoughQuestions(
                easyPool,
                EasyQuestionsPerGame,
                "dễ");

            EnsureEnoughQuestions(
                mediumPool,
                MediumQuestionsPerGame,
                "trung bình");

            EnsureEnoughQuestions(
                hardPool,
                HardQuestionsPerGame,
                "khó");

            // Chỉ trộn bên trong từng tầng.
            ShuffleItems(easyPool);
            ShuffleItems(mediumPool);
            ShuffleItems(hardPool);

            List<Question> gameQuestions =
                new List<Question>(QuestionsPerGame);

            // Lấy các phần tử đầu sau khi đã trộn.
            // Mỗi câu được thêm đúng một lần.
            AddSelectedQuestions(
                gameQuestions,
                easyPool,
                EasyQuestionsPerGame);

            AddSelectedQuestions(
                gameQuestions,
                mediumPool,
                MediumQuestionsPerGame);

            AddSelectedQuestions(
                gameQuestions,
                hardPool,
                HardQuestionsPerGame);

            // Không trộn lại danh sách đã ghép:
            // cần giữ nguyên các đoạn dễ, trung bình, khó.
            return gameQuestions.AsReadOnly();
        }

        // Lấy những câu đúng tầng và không thuộc danh sách cần tránh.
        // Danh sách trả về là danh sách mới, không phải danh sách gốc.
        private List<Question> GetAvailableQuestions(
            QuestionDifficulty difficulty,
            HashSet<string> excludedIds)
        {
            List<Question> result = new List<Question>();

            foreach (Question question in _questions)
            {
                if (question.Difficulty == difficulty &&
                    !excludedIds.Contains(question.Id))
                {
                    result.Add(question);
                }
            }

            return result;
        }

        // Báo rõ tầng nào thiếu câu sau khi loại các mã cần tránh.
        private static void EnsureEnoughQuestions(
            List<Question> pool,
            int requiredCount,
            string difficultyName)
        {
            if (pool.Count < requiredCount)
            {
                throw new InvalidOperationException(
                    "Không đủ câu " + difficultyName +
                    " sau khi loại các mã cần tránh. Cần " +
                    requiredCount + " câu nhưng chỉ còn " +
                    pool.Count + " câu.");
            }
        }

        // Thêm số lượng câu đã yêu cầu vào danh sách của ván.
        // Mỗi câu được sao chép và trộn đáp án trước khi thêm.
        private void AddSelectedQuestions(
            List<Question> destination,
            List<Question> shuffledPool,
            int count)
        {
            for (int i = 0; i < count; i++)
            {
                Question shuffledQuestion =
                    ShuffleOptions(shuffledPool[i]);

                destination.Add(shuffledQuestion);
            }
        }

        // Trộn đáp án bằng cách trộn các vị trí gốc.
        // Nhờ theo dõi vị trí gốc, luôn xác định được
        // đáp án đúng đã chuyển đến đâu.
        private Question ShuffleOptions(Question original)
        {
            string[] originalOptions = original.Options;

            // Ban đầu, mỗi vị trí trỏ đến chính nó.
            List<int> originalIndexes = new List<int>
            {
                0, 1, 2, 3
            };

            ShuffleItems(originalIndexes);

            string[] shuffledOptions = new string[4];
            int newCorrectAnswerIndex = -1;

            for (int newIndex = 0; newIndex < 4; newIndex++)
            {
                // Xác định đáp án gốc nào được đưa vào vị trí mới.
                int oldIndex = originalIndexes[newIndex];

                shuffledOptions[newIndex] =
                    originalOptions[oldIndex];

                // Nếu đang chuyển đáp án đúng,
                // ghi nhận ngay vị trí mới của nó.
                if (oldIndex == original.CorrectAnswerIndex)
                {
                    newCorrectAnswerIndex = newIndex;
                }
            }

            // Kiểm tra bảo vệ trước khi tạo đối tượng mới.
            if (newCorrectAnswerIndex == -1)
            {
                throw new InvalidOperationException(
                    "Không xác định được đáp án đúng sau khi trộn: " +
                    original.Id);
            }

            // Giữ nguyên mã, nội dung và tầng độ khó.
            // Chỉ thứ tự đáp án và chỉ số đúng có thể thay đổi.
            return new Question(
                original.Id,
                original.Text,
                shuffledOptions,
                newCorrectAnswerIndex,
                original.Difficulty);
        }

        // Trộn danh sách bằng cách đi từ cuối về đầu.
        // Tại mỗi vị trí, chọn ngẫu nhiên một vị trí từ đầu
        // đến vị trí hiện tại rồi hoán đổi hai phần tử.
        //
        // T đại diện cho loại phần tử:
        // cùng một hàm dùng được cho câu hỏi và cho số nguyên.
        private void ShuffleItems<T>(IList<T> items)
        {
            for (int i = items.Count - 1; i > 0; i--)
            {
                int randomIndex = _random.Next(i + 1);

                T temporary = items[i];
                items[i] = items[randomIndex];
                items[randomIndex] = temporary;
            }
        }

        // Chọn đúng một bộ dữ liệu cho ngân hàng.
        // Mỗi chủ đề có 9 câu dễ, 12 câu trung bình và 9 câu khó.
        private static List<Question> CreateDefaultQuestions(QuestionTopic topic)
        {
            switch (topic)
            {
                case QuestionTopic.Programming:
                    return CreateProgrammingQuestions();

                case QuestionTopic.Networking:
                    return CreateNetworkingQuestions();

                case QuestionTopic.Databases:
                    return CreateDatabasesQuestions();

                case QuestionTopic.HardwareOperatingSystems:
                    return CreateHardwareOperatingSystemsQuestions();

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(topic),
                        "Chủ đề không hợp lệ.");
            }
        }

        // CHỦ ĐỀ: LẬP TRÌNH C#
        // Mã câu có tiền tố riêng để không trùng giữa các chủ đề.
        private static List<Question> CreateProgrammingQuestions()
        {
            return new List<Question>
            {
                // DỄ: 9 câu.
                new Question(
                    "CS-D01",
                    "Tệp chứa mã nguồn C# thường có phần mở rộng nào?",
                    new string[]
                    {
                        ".cs",
                        ".jpg",
                        ".mp3",
                        ".xlsx"
                    },
                    0,
                    QuestionDifficulty.Easy),

                new Question(
                    "CS-D02",
                    "Trong ứng dụng dòng lệnh C#, lệnh nào ghi một dòng chữ ra màn hình?",
                    new string[]
                    {
                        "Array.Sort",
                        "Console.WriteLine",
                        "Console.ReadLine",
                        "Math.Abs"
                    },
                    1,
                    QuestionDifficulty.Easy),

                new Question(
                    "CS-D03",
                    "Ký hiệu nào bắt đầu chú thích một dòng trong C#?",
                    new string[]
                    {
                        "**",
                        "??",
                        "//",
                        "<!--"
                    },
                    2,
                    QuestionDifficulty.Easy),

                new Question(
                    "CS-D04",
                    "Trong câu lệnh int tuoi = 18;, dấu = có nhiệm vụ gì?",
                    new string[]
                    {
                        "So sánh hai giá trị",
                        "Kết thúc chương trình",
                        "Khai báo vòng lặp",
                        "Gán giá trị cho biến"
                    },
                    3,
                    QuestionDifficulty.Easy),

                new Question(
                    "CS-D05",
                    "Cách khai báo nào lưu được tên người chơi bằng chuỗi chữ trong C#?",
                    new string[]
                    {
                        "string ten = \"Hào\";",
                        "int ten = \"Hào\";",
                        "bool ten = \"Hào\";",
                        "double ten = \"Hào\";"
                    },
                    0,
                    QuestionDifficulty.Easy),

                new Question(
                    "CS-D06",
                    "Từ khóa nào dùng để rẽ nhánh theo một điều kiện trong C#?",
                    new string[]
                    {
                        "class",
                        "if",
                        "using",
                        "namespace"
                    },
                    1,
                    QuestionDifficulty.Easy),

                new Question(
                    "CS-D07",
                    "Từ khóa nào mở đầu một vòng lặp đếm trong C#?",
                    new string[]
                    {
                        "public",
                        "new",
                        "for",
                        "return"
                    },
                    2,
                    QuestionDifficulty.Easy),

                new Question(
                    "CS-D08",
                    "Trong C#, từ khóa class dùng để khai báo gì?",
                    new string[]
                    {
                        "Một tệp âm thanh",
                        "Một địa chỉ mạng",
                        "Một kiểu hình ảnh",
                        "Một lớp mô tả đối tượng"
                    },
                    3,
                    QuestionDifficulty.Easy),

                new Question(
                    "CS-D09",
                    "Trong C#, namespace giúp tổ chức mã theo cách nào?",
                    new string[]
                    {
                        "Gom các kiểu vào vùng tên để phân biệt chúng",
                        "Tự tăng tốc mọi vòng lặp",
                        "Tự lưu dữ liệu vào ổ đĩa",
                        "Tự chuyển chuỗi thành số"
                    },
                    0,
                    QuestionDifficulty.Easy),

                // TRUNG BÌNH: 12 câu.
                new Question(
                    "CS-TB01",
                    "Trong C#, biểu thức \"4\" + 2 tạo ra chuỗi nào?",
                    new string[]
                    {
                        "4 + 2",
                        "42",
                        "6",
                        "24"
                    },
                    1,
                    QuestionDifficulty.Medium),

                new Question(
                    "CS-TB02",
                    "Sau đoạn C#: int tong = 0; for (int i = 1; i <= 4; i++) tong += i; biến tong bằng bao nhiêu?",
                    new string[]
                    {
                        "4",
                        "15",
                        "10",
                        "6"
                    },
                    2,
                    QuestionDifficulty.Medium),

                new Question(
                    "CS-TB03",
                    "Với int[] a = { 3, 7, 9, 12 }; trong C#, a.Length bằng bao nhiêu?",
                    new string[]
                    {
                        "3",
                        "12",
                        "31",
                        "4"
                    },
                    3,
                    QuestionDifficulty.Medium),

                new Question(
                    "CS-TB04",
                    "Ngay sau câu lệnh int[] a = new int[3]; trong C#, a[1] có giá trị nào?",
                    new string[]
                    {
                        "0",
                        "1",
                        "3",
                        "null"
                    },
                    0,
                    QuestionDifficulty.Medium),

                new Question(
                    "CS-TB05",
                    "Với danh sách C# List<int> đang rỗng, gọi Add(5) rồi Add(8) thì Count bằng bao nhiêu?",
                    new string[]
                    {
                        "13",
                        "2",
                        "0",
                        "5"
                    },
                    1,
                    QuestionDifficulty.Medium),

                new Question(
                    "CS-TB06",
                    "Khi được thực thi trong một phương thức C#, return có tác dụng gì?",
                    new string[]
                    {
                        "Luôn quay về đầu vòng lặp",
                        "Tự ghi dữ liệu xuống tệp",
                        "Kết thúc lần gọi phương thức hiện tại và có thể trả về giá trị",
                        "Luôn kết thúc toàn bộ ứng dụng"
                    },
                    2,
                    QuestionDifficulty.Medium),

                new Question(
                    "CS-TB07",
                    "Mức truy cập nào trong C# giới hạn thành viên trong phạm vi kiểu chứa nó?",
                    new string[]
                    {
                        "public",
                        "internal",
                        "protected internal",
                        "private"
                    },
                    3,
                    QuestionDifficulty.Medium),

                new Question(
                    "CS-TB08",
                    "Tên hàm khởi tạo của một lớp C# phải như thế nào?",
                    new string[]
                    {
                        "Trùng tên lớp",
                        "Luôn là Main",
                        "Luôn là Create",
                        "Trùng tên vùng chứa namespace"
                    },
                    0,
                    QuestionDifficulty.Medium),

                new Question(
                    "CS-TB09",
                    "Trong cấu trúc try-catch của C#, khối catch dùng để làm gì?",
                    new string[]
                    {
                        "Tự động bỏ mọi điều kiện if",
                        "Xử lý ngoại lệ phù hợp phát sinh từ khối try",
                        "Lặp lại chương trình vô hạn",
                        "Khai báo tên lớp mới"
                    },
                    1,
                    QuestionDifficulty.Medium),

                new Question(
                    "CS-TB10",
                    "Trong C#: string s = \"abc\"; s.ToUpper(); Sau đó s vẫn chứa gì?",
                    new string[]
                    {
                        "Trở thành chuỗi rỗng",
                        "Đã thành \"abcABC\"",
                        "Vẫn là \"abc\"",
                        "Đã thành \"ABC\""
                    },
                    2,
                    QuestionDifficulty.Medium),

                new Question(
                    "CS-TB11",
                    "Trong C#: int x = 5; x += 3; Giá trị cuối của x là gì?",
                    new string[]
                    {
                        "2",
                        "15",
                        "53",
                        "8"
                    },
                    3,
                    QuestionDifficulty.Medium),

                new Question(
                    "CS-TB12",
                    "Một phương thức C# được khai báo trả về void có ý nghĩa gì?",
                    new string[]
                    {
                        "Không trả về giá trị cho nơi gọi",
                        "Luôn trả về số 0",
                        "Luôn trả về null",
                        "Không được chứa câu lệnh"
                    },
                    0,
                    QuestionDifficulty.Medium),

                // KHÓ: 9 câu.
                new Question(
                    "CS-K01",
                    "Trong C#: int x = 3; int y = x++; Sau hai lệnh, cặp giá trị (x, y) là gì?",
                    new string[]
                    {
                        "(3, 3)",
                        "(4, 3)",
                        "(4, 4)",
                        "(3, 4)"
                    },
                    1,
                    QuestionDifficulty.Hard),

                new Question(
                    "CS-K02",
                    "Trong C#: int a = 0; bool b = false && (++a > 0); Sau đó a bằng bao nhiêu?",
                    new string[]
                    {
                        "-1",
                        "Đoạn lệnh không biên dịch được",
                        "0",
                        "1"
                    },
                    2,
                    QuestionDifficulty.Hard),

                new Question(
                    "CS-K03",
                    "Theo thứ tự ưu tiên toán tử C#, biểu thức true || false && false có kết quả gì?",
                    new string[]
                    {
                        "false",
                        "null",
                        "Lỗi vì thiếu dấu ngoặc",
                        "true"
                    },
                    3,
                    QuestionDifficulty.Hard),

                new Question(
                    "CS-K04",
                    "Trong C#, hai vòng for lồng nhau chạy i từ 0 đến dưới 2 và j từ 0 đến dưới 3. Nếu mỗi lượt trong cùng tăng dem thêm 1, từ dem = 0 thì kết quả là gì?",
                    new string[]
                    {
                        "6",
                        "5",
                        "3",
                        "9"
                    },
                    0,
                    QuestionDifficulty.Hard),

                new Question(
                    "CS-K05",
                    "Trong C#: int[] a = { 2, 4 }; int[] b = (int[])a.Clone(); b[0] = 8; Sau đó a[0] bằng bao nhiêu?",
                    new string[]
                    {
                        "0",
                        "2",
                        "8",
                        "4"
                    },
                    1,
                    QuestionDifficulty.Hard),

                new Question(
                    "CS-K06",
                    "Trong C#, hàm Tang(int x) chỉ thực hiện x++;. Với int n = 5; sau khi gọi Tang(n), n bằng bao nhiêu?",
                    new string[]
                    {
                        "0",
                        "Không xác định",
                        "5",
                        "6"
                    },
                    2,
                    QuestionDifficulty.Hard),

                new Question(
                    "CS-K07",
                    "Hàm C# F(int n) trả 1 khi n <= 1, còn lại trả n * F(n - 1). F(4) bằng bao nhiêu?",
                    new string[]
                    {
                        "10",
                        "16",
                        "4",
                        "24"
                    },
                    3,
                    QuestionDifficulty.Hard),

                new Question(
                    "CS-K08",
                    "Trong C#: int? n = null; int x = n ?? 7; Giá trị của x là gì?",
                    new string[]
                    {
                        "7",
                        "0",
                        "null",
                        "Đoạn lệnh luôn ném ngoại lệ"
                    },
                    0,
                    QuestionDifficulty.Hard),

                new Question(
                    "CS-K09",
                    "Trong C#: int a = 9, b = 4; double k = (double)a / b; Giá trị của k là gì?",
                    new string[]
                    {
                        "0,25",
                        "2,25",
                        "2",
                        "3"
                    },
                    1,
                    QuestionDifficulty.Hard),

            };
        }

        // CHỦ ĐỀ: MẠNG MÁY TÍNH VÀ INTERNET
        // Mã câu có tiền tố riêng để không trùng giữa các chủ đề.
        private static List<Question> CreateNetworkingQuestions()
        {
            return new List<Question>
            {
                // DỄ: 9 câu.
                new Question(
                    "NET-D01",
                    "Internet được mô tả đúng nhất là gì?",
                    new string[]
                    {
                        "Hệ thống nhiều mạng máy tính kết nối với nhau",
                        "Một phần mềm soạn thảo văn bản",
                        "Một loại ổ cứng",
                        "Một chiếc máy chủ duy nhất"
                    },
                    0,
                    QuestionDifficulty.Easy),

                new Question(
                    "NET-D02",
                    "Mạng LAN, tức mạng cục bộ, thường phục vụ phạm vi nào?",
                    new string[]
                    {
                        "Chỉ phần bên trong một tệp",
                        "Một phòng, tòa nhà hoặc khu vực nhỏ",
                        "Toàn bộ các hành tinh",
                        "Chỉ một phím trên bàn phím"
                    },
                    1,
                    QuestionDifficulty.Easy),

                new Question(
                    "NET-D03",
                    "Bộ định tuyến, còn gọi là router, có nhiệm vụ chính nào?",
                    new string[]
                    {
                        "Lưu mọi mật khẩu của người dùng",
                        "Biến màn hình thành máy chiếu",
                        "Chuyển gói tin giữa các mạng",
                        "In tài liệu ra giấy"
                    },
                    2,
                    QuestionDifficulty.Easy),

                new Question(
                    "NET-D04",
                    "Địa chỉ IP chủ yếu được dùng để làm gì trong mạng?",
                    new string[]
                    {
                        "Đo nhiệt độ bộ xử lý",
                        "Mã hóa mọi tệp trên máy",
                        "Xác định màu của màn hình",
                        "Định địa chỉ các giao diện mạng để trao đổi gói tin"
                    },
                    3,
                    QuestionDifficulty.Easy),

                new Question(
                    "NET-D05",
                    "Phần mềm nào dùng để truy cập và xem các trang web?",
                    new string[]
                    {
                        "Trình duyệt web",
                        "Trình điều khiển máy in",
                        "Phần mềm nén tệp",
                        "Công cụ định dạng ổ đĩa"
                    },
                    0,
                    QuestionDifficulty.Easy),

                new Question(
                    "NET-D06",
                    "URL, tức địa chỉ tài nguyên trên web, có vai trò gì?",
                    new string[]
                    {
                        "Xóa lịch sử đăng nhập",
                        "Chỉ ra địa chỉ của tài nguyên cần truy cập",
                        "Thay thế bộ nhớ RAM",
                        "Tăng số lõi bộ xử lý"
                    },
                    1,
                    QuestionDifficulty.Easy),

                new Question(
                    "NET-D07",
                    "Wi-Fi cho phép thiết bị kết nối mạng bằng phương tiện nào?",
                    new string[]
                    {
                        "Chỉ bằng dây nguồn điện",
                        "Bằng giấy in",
                        "Sóng vô tuyến",
                        "Chỉ bằng cáp quang cắm trực tiếp"
                    },
                    2,
                    QuestionDifficulty.Easy),

                new Question(
                    "NET-D08",
                    "Trong mạng Ethernet có dây thông dụng, máy tính thường nối với bộ chuyển mạch bằng gì?",
                    new string[]
                    {
                        "Cáp âm thanh của tai nghe",
                        "Dây nối quạt tản nhiệt",
                        "Dây của nút nguồn",
                        "Cáp mạng"
                    },
                    3,
                    QuestionDifficulty.Easy),

                new Question(
                    "NET-D09",
                    "Tải xuống một tệp từ máy chủ về máy cá nhân nghĩa là gì?",
                    new string[]
                    {
                        "Nhận dữ liệu từ máy chủ về máy cá nhân",
                        "Gửi dữ liệu từ máy cá nhân lên máy chủ",
                        "Xóa tệp trên cả hai máy",
                        "Đổi tên máy chủ"
                    },
                    0,
                    QuestionDifficulty.Easy),

                // TRUNG BÌNH: 12 câu.
                new Question(
                    "NET-TB01",
                    "DNS, hệ thống phân giải tên miền, hỗ trợ truy cập web bằng cách nào?",
                    new string[]
                    {
                        "Thay thế giao thức truyền dữ liệu",
                        "Tra cứu tên miền để tìm địa chỉ IP tương ứng",
                        "Tăng dung lượng ổ cứng",
                        "Tự sửa mọi lỗi của trang web"
                    },
                    1,
                    QuestionDifficulty.Medium),

                new Question(
                    "NET-TB02",
                    "DHCP thường giúp máy vừa tham gia mạng nhận thông tin nào một cách tự động?",
                    new string[]
                    {
                        "Danh sách mọi mật khẩu trong mạng",
                        "Số sê-ri của màn hình",
                        "Địa chỉ IP và các thông số cấu hình mạng",
                        "Mã nguồn của hệ điều hành"
                    },
                    2,
                    QuestionDifficulty.Medium),

                new Question(
                    "NET-TB03",
                    "Đặc điểm nào đúng với TCP khi kết nối hoạt động bình thường?",
                    new string[]
                    {
                        "Không sử dụng số thứ tự dữ liệu",
                        "Không bao giờ phát hiện mất dữ liệu",
                        "Chỉ truyền được hình ảnh",
                        "Cung cấp luồng dữ liệu tin cậy và đúng thứ tự"
                    },
                    3,
                    QuestionDifficulty.Medium),

                new Question(
                    "NET-TB04",
                    "Điều nào TCP có nhưng bản thân UDP không cung cấp sẵn?",
                    new string[]
                    {
                        "Cơ chế xác nhận và truyền lại để có luồng dữ liệu tin cậy",
                        "Khả năng dùng số cổng",
                        "Khả năng truyền dữ liệu qua IP",
                        "Khả năng gửi dữ liệu giữa các máy"
                    },
                    0,
                    QuestionDifficulty.Medium),

                new Question(
                    "NET-TB05",
                    "HTTPS sử dụng TLS để bảo vệ điều gì?",
                    new string[]
                    {
                        "Dung lượng pin của thiết bị",
                        "Tính bí mật và toàn vẹn của dữ liệu trên đường truyền",
                        "Mọi tệp đã lưu trên máy người dùng",
                        "Chắc chắn mọi nội dung của trang đều trung thực"
                    },
                    1,
                    QuestionDifficulty.Medium),

                new Question(
                    "NET-TB06",
                    "Mã trạng thái HTTP 404 thường có nghĩa gì?",
                    new string[]
                    {
                        "Máy chủ yêu cầu đổi màn hình",
                        "Kết nối luôn được mã hóa hai lần",
                        "Không tìm thấy tài nguyên được yêu cầu",
                        "Yêu cầu thành công và có dữ liệu"
                    },
                    2,
                    QuestionDifficulty.Medium),

                new Question(
                    "NET-TB07",
                    "Khi URL HTTPS không ghi rõ cổng, cổng mặc định là số nào?",
                    new string[]
                    {
                        "21",
                        "25",
                        "110",
                        "443"
                    },
                    3,
                    QuestionDifficulty.Medium),

                new Question(
                    "NET-TB08",
                    "Một địa chỉ IPv4 gồm bao nhiêu bit?",
                    new string[]
                    {
                        "32 bit",
                        "16 bit",
                        "64 bit",
                        "128 bit"
                    },
                    0,
                    QuestionDifficulty.Medium),

                new Question(
                    "NET-TB09",
                    "Mặt nạ mạng con giúp xác định điều gì trong một địa chỉ IPv4?",
                    new string[]
                    {
                        "Dung lượng tệp đang tải",
                        "Phần mạng và phần máy trong địa chỉ",
                        "Màu của dây cáp",
                        "Tên người đang dùng máy"
                    },
                    1,
                    QuestionDifficulty.Medium),

                new Question(
                    "NET-TB10",
                    "Khi không có tuyến cụ thể phù hợp, máy thường gửi gói tin ra ngoài mạng cục bộ qua đâu?",
                    new string[]
                    {
                        "Máy in mặc định",
                        "Thư mục tải xuống",
                        "Cổng mặc định",
                        "Bộ nhớ đệm bàn phím"
                    },
                    2,
                    QuestionDifficulty.Medium),

                new Question(
                    "NET-TB11",
                    "Địa chỉ MAC thường được dùng ở tầng liên kết dữ liệu để làm gì?",
                    new string[]
                    {
                        "Thay thế mọi tên miền trên Internet",
                        "Đo tốc độ quay của quạt",
                        "Lưu nội dung trang web",
                        "Định địa chỉ giao diện khi chuyển khung dữ liệu trong liên kết cục bộ"
                    },
                    3,
                    QuestionDifficulty.Medium),

                new Question(
                    "NET-TB12",
                    "Độ trễ mạng phản ánh đại lượng nào?",
                    new string[]
                    {
                        "Thời gian dữ liệu hoặc phản hồi mất để đi qua mạng",
                        "Dung lượng tối đa của ổ cứng",
                        "Số phím trên bàn phím",
                        "Số màu mà màn hình hiển thị"
                    },
                    0,
                    QuestionDifficulty.Medium),

                // KHÓ: 9 câu.
                new Question(
                    "NET-K01",
                    "Máy có địa chỉ 192.168.10.77/26 thuộc địa chỉ mạng con nào?",
                    new string[]
                    {
                        "192.168.10.128",
                        "192.168.10.64",
                        "192.168.10.0",
                        "192.168.10.77"
                    },
                    1,
                    QuestionDifficulty.Hard),

                new Question(
                    "NET-K02",
                    "Một mạng con IPv4 /26 có bao nhiêu địa chỉ gán cho máy, khi loại địa chỉ mạng và quảng bá?",
                    new string[]
                    {
                        "30",
                        "126",
                        "62",
                        "64"
                    },
                    2,
                    QuestionDifficulty.Hard),

                new Question(
                    "NET-K03",
                    "Địa chỉ quảng bá của mạng IPv4 192.168.5.0/27 là gì?",
                    new string[]
                    {
                        "192.168.5.32",
                        "192.168.5.255",
                        "192.168.5.1",
                        "192.168.5.31"
                    },
                    3,
                    QuestionDifficulty.Hard),

                new Question(
                    "NET-K04",
                    "Mặt nạ IPv4 255.255.255.0 tương ứng độ dài tiền tố nào?",
                    new string[]
                    {
                        "/24",
                        "/16",
                        "/25",
                        "/32"
                    },
                    0,
                    QuestionDifficulty.Hard),

                new Question(
                    "NET-K05",
                    "Trình tự bắt tay ba bước để mở kết nối TCP thông thường là gì?",
                    new string[]
                    {
                        "SYN → RST → FIN",
                        "SYN → SYN-ACK → ACK",
                        "ACK → FIN → SYN",
                        "FIN → FIN-ACK → ACK"
                    },
                    1,
                    QuestionDifficulty.Hard),

                new Question(
                    "NET-K06",
                    "NAT, tức chuyển đổi địa chỉ mạng, chủ yếu thực hiện việc gì?",
                    new string[]
                    {
                        "Thay thế DNS trên mọi máy",
                        "Bảo đảm không bao giờ mất gói tin",
                        "Thay đổi địa chỉ IP trong gói tin khi đi qua thiết bị chuyển đổi",
                        "Mã hóa toàn bộ nội dung mọi gói tin"
                    },
                    2,
                    QuestionDifficulty.Hard),

                new Question(
                    "NET-K07",
                    "Trong IPv4, bộ định tuyến xử lý thế nào khi giảm TTL của gói tin xuống 0?",
                    new string[]
                    {
                        "Tự tăng TTL lên giá trị ban đầu",
                        "Biến gói tin thành địa chỉ MAC",
                        "Gửi gói tin đồng thời tới mọi tên miền",
                        "Loại bỏ gói tin để hạn chế việc đi vòng vô hạn"
                    },
                    3,
                    QuestionDifficulty.Hard),

                new Question(
                    "NET-K08",
                    "DNS đã trả về IP, nhưng chứng chỉ TLS của máy chủ không khớp tên miền đang truy cập. Lỗi trực tiếp thuộc bước nào?",
                    new string[]
                    {
                        "Xác thực danh tính máy chủ khi thiết lập kết nối bảo mật",
                        "Đếm số lõi bộ xử lý",
                        "Phân vùng ổ đĩa",
                        "Chuyển chữ hoa thành chữ thường"
                    },
                    0,
                    QuestionDifficulty.Hard),

                new Question(
                    "NET-K09",
                    "Bỏ qua mọi chi phí giao thức, đường truyền 40 triệu bit/giây tải tệp 10 triệu byte mất ít nhất bao lâu?",
                    new string[]
                    {
                        "20 giây",
                        "2 giây",
                        "0,25 giây",
                        "8 giây"
                    },
                    1,
                    QuestionDifficulty.Hard),

            };
        }

        // CHỦ ĐỀ: CƠ SỞ DỮ LIỆU VÀ SQL
        // Mã câu có tiền tố riêng để không trùng giữa các chủ đề.
        private static List<Question> CreateDatabasesQuestions()
        {
            return new List<Question>
            {
                // DỄ: 9 câu.
                new Question(
                    "DB-D01",
                    "Trong cơ sở dữ liệu quan hệ, bảng thường tổ chức dữ liệu theo dạng nào?",
                    new string[]
                    {
                        "Các hàng và các cột",
                        "Chỉ các hình tròn",
                        "Chỉ một đoạn âm thanh",
                        "Chỉ các cửa sổ giao diện"
                    },
                    0,
                    QuestionDifficulty.Easy),

                new Question(
                    "DB-D02",
                    "Một hàng trong bảng SinhVien thường biểu diễn điều gì?",
                    new string[]
                    {
                        "Một cáp mạng",
                        "Một bản ghi sinh viên",
                        "Tên của toàn bộ cơ sở dữ liệu",
                        "Một phần mềm diệt vi-rút"
                    },
                    1,
                    QuestionDifficulty.Easy),

                new Question(
                    "DB-D03",
                    "Trong bảng SinhVien, cột HoTen biểu diễn điều gì?",
                    new string[]
                    {
                        "Một loại bộ nhớ",
                        "Một giao thức mạng",
                        "Một thuộc tính của các sinh viên",
                        "Toàn bộ hệ điều hành"
                    },
                    2,
                    QuestionDifficulty.Easy),

                new Question(
                    "DB-D04",
                    "Trong SQL, ngôn ngữ truy vấn dữ liệu, lệnh nào dùng để đọc các hàng từ bảng?",
                    new string[]
                    {
                        "INSERT",
                        "UPDATE",
                        "DELETE",
                        "SELECT"
                    },
                    3,
                    QuestionDifficulty.Easy),

                new Question(
                    "DB-D05",
                    "Lệnh SQL nào dùng để thêm một hàng mới vào bảng?",
                    new string[]
                    {
                        "INSERT",
                        "SELECT",
                        "DROP",
                        "COMMIT"
                    },
                    0,
                    QuestionDifficulty.Easy),

                new Question(
                    "DB-D06",
                    "Lệnh SQL nào dùng để sửa giá trị của các hàng đã có?",
                    new string[]
                    {
                        "ROLLBACK",
                        "UPDATE",
                        "SELECT",
                        "CREATE"
                    },
                    1,
                    QuestionDifficulty.Easy),

                new Question(
                    "DB-D07",
                    "Lệnh SQL nào dùng để xóa các hàng khỏi bảng?",
                    new string[]
                    {
                        "INSERT",
                        "GRANT",
                        "DELETE",
                        "SELECT"
                    },
                    2,
                    QuestionDifficulty.Easy),

                new Question(
                    "DB-D08",
                    "Trong truy vấn SQL, WHERE thường dùng để làm gì?",
                    new string[]
                    {
                        "Đổi màu chữ của kết quả",
                        "Đặt lại đồng hồ hệ thống",
                        "Tạo kết nối Wi-Fi",
                        "Lọc các hàng theo điều kiện"
                    },
                    3,
                    QuestionDifficulty.Easy),

                new Question(
                    "DB-D09",
                    "Khóa chính của một bảng cần thỏa mãn tính chất nào?",
                    new string[]
                    {
                        "Xác định duy nhất mỗi hàng và không nhận NULL",
                        "Luôn có đúng 10 ký tự",
                        "Luôn là tên người",
                        "Được trùng và bỏ trống tùy ý"
                    },
                    0,
                    QuestionDifficulty.Easy),

                // TRUNG BÌNH: 12 câu.
                new Question(
                    "DB-TB01",
                    "ORDER BY Diem ASC sắp xếp các điểm số không NULL theo thứ tự nào?",
                    new string[]
                    {
                        "Theo độ dài tên bảng",
                        "Tăng dần",
                        "Giảm dần",
                        "Luôn ngẫu nhiên"
                    },
                    1,
                    QuestionDifficulty.Medium),

                new Question(
                    "DB-TB02",
                    "Muốn sắp xếp giá sản phẩm từ cao xuống thấp, dùng cách nào?",
                    new string[]
                    {
                        "GROUP BY Gia",
                        "WHERE Gia IS NULL",
                        "ORDER BY Gia DESC",
                        "ORDER BY Gia ASC"
                    },
                    2,
                    QuestionDifficulty.Medium),

                new Question(
                    "DB-TB03",
                    "SELECT DISTINCT ThanhPho loại bỏ điều gì khỏi kết quả?",
                    new string[]
                    {
                        "Mọi thành phố có chữ hoa",
                        "Mọi hàng trong bảng gốc",
                        "Tên của cột ThanhPho",
                        "Các giá trị thành phố bị lặp trong kết quả"
                    },
                    3,
                    QuestionDifficulty.Medium),

                new Question(
                    "DB-TB04",
                    "Một bảng có 3 hàng, trong đó một hàng có cột Diem là NULL. SELECT COUNT(*) trên bảng trả về bao nhiêu?",
                    new string[]
                    {
                        "3",
                        "2",
                        "1",
                        "0"
                    },
                    0,
                    QuestionDifficulty.Medium),

                new Question(
                    "DB-TB05",
                    "Cột SoLuong có ba giá trị 10, 20, 30 và không có NULL. SUM(SoLuong) bằng bao nhiêu?",
                    new string[]
                    {
                        "3",
                        "60",
                        "30",
                        "20"
                    },
                    1,
                    QuestionDifficulty.Medium),

                new Question(
                    "DB-TB06",
                    "Cột Diem có ba giá trị 4, 8, 12 và không có NULL. AVG(Diem) bằng bao nhiêu?",
                    new string[]
                    {
                        "12",
                        "4",
                        "8",
                        "24"
                    },
                    2,
                    QuestionDifficulty.Medium),

                new Question(
                    "DB-TB07",
                    "Khóa ngoại thường được dùng để duy trì điều gì?",
                    new string[]
                    {
                        "Độ sáng của màn hình",
                        "Tốc độ quạt máy tính",
                        "Màu của từng hàng",
                        "Tính hợp lệ của liên kết tới khóa được tham chiếu ở bảng liên quan"
                    },
                    3,
                    QuestionDifficulty.Medium),

                new Question(
                    "DB-TB08",
                    "INNER JOIN với điều kiện nối tạo ra các hàng kết quả như thế nào?",
                    new string[]
                    {
                        "Các cặp hàng thỏa mãn điều kiện nối",
                        "Luôn giữ mọi hàng của cả hai bảng dù không khớp",
                        "Chỉ giữ hàng không có khóa",
                        "Luôn xóa các hàng trùng khỏi bảng gốc"
                    },
                    0,
                    QuestionDifficulty.Medium),

                new Question(
                    "DB-TB09",
                    "LEFT JOIN bảo đảm điều gì đối với bảng bên trái?",
                    new string[]
                    {
                        "Xóa các hàng bên trái không khớp",
                        "Giữ mọi hàng bên trái kể cả khi không có hàng khớp bên phải",
                        "Chỉ giữ hàng bên phải",
                        "Luôn trả đúng một hàng"
                    },
                    1,
                    QuestionDifficulty.Medium),

                new Question(
                    "DB-TB10",
                    "GROUP BY trong SQL có vai trò gì?",
                    new string[]
                    {
                        "Sắp xếp lại các cột trên ổ đĩa",
                        "Luôn xóa bản ghi bị lặp",
                        "Gom các hàng theo giá trị để tính kết quả theo nhóm",
                        "Đổi tên máy chủ"
                    },
                    2,
                    QuestionDifficulty.Medium),

                new Question(
                    "DB-TB11",
                    "Muốn lọc các nhóm có COUNT(*) > 5 sau GROUP BY, dùng mệnh đề nào?",
                    new string[]
                    {
                        "WHERE COUNT(*) > 5",
                        "ORDER BY COUNT(*) > 5",
                        "INSERT COUNT(*) > 5",
                        "HAVING COUNT(*) > 5"
                    },
                    3,
                    QuestionDifficulty.Medium),

                new Question(
                    "DB-TB12",
                    "Điều kiện SQL nào kiểm tra cột Email chưa có giá trị, tức là NULL?",
                    new string[]
                    {
                        "Email IS NULL",
                        "Email = NULL",
                        "Email == NULL",
                        "Email = 'NULL'"
                    },
                    0,
                    QuestionDifficulty.Medium),

                // KHÓ: 9 câu.
                new Question(
                    "DB-K01",
                    "Cột Diem có các giá trị 10, NULL, 20. SELECT COUNT(Diem) trả về bao nhiêu?",
                    new string[]
                    {
                        "NULL",
                        "2",
                        "3",
                        "0"
                    },
                    1,
                    QuestionDifficulty.Hard),

                new Question(
                    "DB-K02",
                    "Cột MaNhom có các giá trị 1, 1, 2, NULL. COUNT(DISTINCT MaNhom) trả về bao nhiêu?",
                    new string[]
                    {
                        "4",
                        "1",
                        "2",
                        "3"
                    },
                    2,
                    QuestionDifficulty.Hard),

                new Question(
                    "DB-K03",
                    "Cột Diem có các giá trị 10, NULL, 20. AVG(Diem) trả về bao nhiêu?",
                    new string[]
                    {
                        "10",
                        "30",
                        "NULL",
                        "15"
                    },
                    3,
                    QuestionDifficulty.Hard),

                new Question(
                    "DB-K04",
                    "Có ba sản phẩm với (Gia, TonKho) lần lượt là (120, 0), (80, 5), (150, 2). WHERE Gia > 100 AND TonKho > 0 giữ lại bao nhiêu hàng?",
                    new string[]
                    {
                        "1",
                        "2",
                        "3",
                        "0"
                    },
                    0,
                    QuestionDifficulty.Hard),

                new Question(
                    "DB-K05",
                    "Bảng KhachHang có hai mã duy nhất 1 và 2. Bảng DonHang có 3 đơn của khách 1 và 1 đơn của khách 2. INNER JOIN theo mã khách, không DISTINCT, trả bao nhiêu hàng?",
                    new string[]
                    {
                        "6",
                        "4",
                        "2",
                        "3"
                    },
                    1,
                    QuestionDifficulty.Hard),

                new Question(
                    "DB-K06",
                    "Bảng A có 3 hàng, bảng B rỗng. Sau A LEFT JOIN B ON A.Id = B.AId, COUNT(B.Id) bằng bao nhiêu?",
                    new string[]
                    {
                        "1",
                        "NULL",
                        "0",
                        "3"
                    },
                    2,
                    QuestionDifficulty.Hard),

                new Question(
                    "DB-K07",
                    "Tính nguyên tử của một giao dịch cơ sở dữ liệu có nghĩa gì?",
                    new string[]
                    {
                        "Mọi truy vấn luôn chạy trong một giây",
                        "Mỗi bảng chỉ được có một cột",
                        "Không cần kiểm tra dữ liệu đầu vào",
                        "Các thay đổi của giao dịch được chấp nhận toàn bộ hoặc không có thay đổi nào được chấp nhận"
                    },
                    3,
                    QuestionDifficulty.Hard),

                new Question(
                    "DB-K08",
                    "Bảng DangKy có khóa chính ghép (MaSinhVien, MaMon). Hai hàng (SV01, C01) và (SV01, C02) có vi phạm riêng quy tắc khóa chính này không?",
                    new string[]
                    {
                        "Không, vì hai cặp giá trị khác nhau",
                        "Có, vì MaSinhVien luôn phải duy nhất riêng lẻ",
                        "Có, vì MaMon luôn phải bằng MaSinhVien",
                        "Có, vì khóa chính không được gồm hai cột"
                    },
                    0,
                    QuestionDifficulty.Hard),

                new Question(
                    "DB-K09",
                    "Vì sao không nên tạo chỉ mục trên mọi cột một cách tùy tiện?",
                    new string[]
                    {
                        "Chỉ mục khiến SQL không còn đọc được bảng",
                        "Chỉ mục chiếm bộ nhớ lưu trữ và làm tăng công việc khi thêm, sửa, xóa dữ liệu",
                        "Chỉ mục luôn làm mọi truy vấn chậm hơn",
                        "Chỉ mục tự xóa khóa chính"
                    },
                    1,
                    QuestionDifficulty.Hard),

            };
        }

        // CHỦ ĐỀ: PHẦN CỨNG VÀ HỆ ĐIỀU HÀNH
        // Mã câu có tiền tố riêng để không trùng giữa các chủ đề.
        private static List<Question> CreateHardwareOperatingSystemsQuestions()
        {
            return new List<Question>
            {
                // DỄ: 9 câu.
                new Question(
                    "SYS-D01",
                    "CPU, tức bộ xử lý trung tâm, có nhiệm vụ chính nào?",
                    new string[]
                    {
                        "Thực thi lệnh và xử lý dữ liệu",
                        "Chỉ hiển thị hình ảnh ra màn hình",
                        "Chỉ lưu tệp khi tắt máy",
                        "Chỉ phát sóng Wi-Fi"
                    },
                    0,
                    QuestionDifficulty.Easy),

                new Question(
                    "SYS-D02",
                    "RAM thông thường được dùng chủ yếu để làm gì?",
                    new string[]
                    {
                        "Làm mát máy tính",
                        "Giữ tạm dữ liệu và chương trình đang được xử lý",
                        "Lưu vĩnh viễn mọi tệp khi mất điện",
                        "Thay thế hoàn toàn bộ xử lý"
                    },
                    1,
                    QuestionDifficulty.Easy),

                new Question(
                    "SYS-D03",
                    "Thiết bị nào sau đây lưu được tệp khi đã tắt nguồn máy tính?",
                    new string[]
                    {
                        "Thanh ghi của CPU",
                        "Bộ nhớ đệm CPU",
                        "Ổ SSD",
                        "RAM thông thường"
                    },
                    2,
                    QuestionDifficulty.Easy),

                new Question(
                    "SYS-D04",
                    "Thiết bị nào chủ yếu dùng để nhập chữ vào máy tính?",
                    new string[]
                    {
                        "Màn hình",
                        "Loa",
                        "Máy chiếu",
                        "Bàn phím"
                    },
                    3,
                    QuestionDifficulty.Easy),

                new Question(
                    "SYS-D05",
                    "Thiết bị nào chủ yếu hiển thị hình ảnh cho người dùng?",
                    new string[]
                    {
                        "Màn hình",
                        "Chuột",
                        "Micro",
                        "Máy quét"
                    },
                    0,
                    QuestionDifficulty.Easy),

                new Question(
                    "SYS-D06",
                    "Bo mạch chủ có vai trò chính nào?",
                    new string[]
                    {
                        "Chỉ lưu mật khẩu mạng",
                        "Kết nối và tạo đường giao tiếp giữa các linh kiện",
                        "Chỉ in tài liệu",
                        "Chỉ thay đổi ảnh nền"
                    },
                    1,
                    QuestionDifficulty.Easy),

                new Question(
                    "SYS-D07",
                    "Hệ điều hành có nhiệm vụ nào sau đây?",
                    new string[]
                    {
                        "Chỉ tạo văn bản",
                        "Thay thế mọi linh kiện vật lý",
                        "Quản lý tài nguyên và hỗ trợ chương trình hoạt động",
                        "Chỉ làm tăng dung lượng ổ cứng"
                    },
                    2,
                    QuestionDifficulty.Easy),

                new Question(
                    "SYS-D08",
                    "Trình điều khiển thiết bị, còn gọi là driver, giúp việc gì?",
                    new string[]
                    {
                        "Biến tệp ảnh thành bộ nhớ RAM",
                        "Thay thế dây nguồn",
                        "Luôn tăng tốc Internet gấp đôi",
                        "Giúp hệ điều hành giao tiếp với thiết bị"
                    },
                    3,
                    QuestionDifficulty.Easy),

                new Question(
                    "SYS-D09",
                    "Quạt và bộ tản nhiệt trong máy tính giúp làm gì?",
                    new string[]
                    {
                        "Đưa nhiệt ra khỏi linh kiện",
                        "Tăng số lượng tệp trong thư mục",
                        "Tạo địa chỉ IP",
                        "Biên dịch mọi ngôn ngữ lập trình"
                    },
                    0,
                    QuestionDifficulty.Easy),

                // TRUNG BÌNH: 12 câu.
                new Question(
                    "SYS-TB01",
                    "Khác biệt cơ bản giữa ổ HDD và SSD là gì?",
                    new string[]
                    {
                        "SSD chỉ hoạt động khi có Internet",
                        "HDD dùng đĩa quay và đầu đọc cơ học, SSD dùng bộ nhớ bán dẫn",
                        "SSD luôn có đĩa quay còn HDD thì không",
                        "HDD chỉ lưu được âm thanh"
                    },
                    1,
                    QuestionDifficulty.Medium),

                new Question(
                    "SYS-TB02",
                    "Bộ nhớ đệm CPU, còn gọi là cache, giúp cải thiện hiệu năng bằng cách nào?",
                    new string[]
                    {
                        "Lưu mọi tệp vĩnh viễn",
                        "Tăng kích thước màn hình",
                        "Giữ dữ liệu hoặc lệnh có khả năng được dùng lại để giảm thời gian truy cập",
                        "Thay thế toàn bộ hệ điều hành"
                    },
                    2,
                    QuestionDifficulty.Medium),

                new Question(
                    "SYS-TB03",
                    "Một lõi xử lý trong CPU được hiểu phù hợp nhất là gì?",
                    new string[]
                    {
                        "Một thư mục trên ổ đĩa",
                        "Một cổng âm thanh",
                        "Một cửa sổ của chương trình",
                        "Một đơn vị xử lý phần cứng có khả năng thực thi lệnh"
                    },
                    3,
                    QuestionDifficulty.Medium),

                new Question(
                    "SYS-TB04",
                    "Luồng thực thi trong hệ điều hành là gì?",
                    new string[]
                    {
                        "Một chuỗi thực thi lệnh thuộc tiến trình",
                        "Một sợi cáp mạng vật lý",
                        "Một loại ổ cứng",
                        "Một bản sao bắt buộc của toàn bộ hệ điều hành"
                    },
                    0,
                    QuestionDifficulty.Medium),

                new Question(
                    "SYS-TB05",
                    "Tiến trình thường bao gồm những gì?",
                    new string[]
                    {
                        "Chỉ một tệp đã xóa",
                        "Chương trình đang chạy cùng không gian địa chỉ và tài nguyên của nó",
                        "Chỉ một ảnh nền",
                        "Chỉ một phím chức năng"
                    },
                    1,
                    QuestionDifficulty.Medium),

                new Question(
                    "SYS-TB06",
                    "Bộ nhớ ảo cung cấp cơ chế nào cho chương trình?",
                    new string[]
                    {
                        "Bảo đảm chương trình không bao giờ hết bộ nhớ",
                        "Tăng số khe RAM trên bo mạch",
                        "Dùng địa chỉ ảo được ánh xạ tới nơi lưu dữ liệu thực tế",
                        "Biến mọi ổ cứng thành RAM có cùng tốc độ"
                    },
                    2,
                    QuestionDifficulty.Medium),

                new Question(
                    "SYS-TB07",
                    "Một byte gồm bao nhiêu bit?",
                    new string[]
                    {
                        "4 bit",
                        "16 bit",
                        "32 bit",
                        "8 bit"
                    },
                    3,
                    QuestionDifficulty.Medium),

                new Question(
                    "SYS-TB08",
                    "Theo đơn vị nhị phân, 1 KiB bằng bao nhiêu byte?",
                    new string[]
                    {
                        "1.024 byte",
                        "1.000 byte",
                        "100 byte",
                        "8.192 byte"
                    },
                    0,
                    QuestionDifficulty.Medium),

                new Question(
                    "SYS-TB09",
                    "SATA là giao tiếp thường dùng để kết nối loại thiết bị nào?",
                    new string[]
                    {
                        "Chỉ loa Bluetooth",
                        "Thiết bị lưu trữ như ổ HDD hoặc SSD SATA",
                        "Chỉ bàn phím không dây",
                        "Chỉ máy chiếu"
                    },
                    1,
                    QuestionDifficulty.Medium),

                new Question(
                    "SYS-TB10",
                    "Đa nhiệm trên hệ điều hành cho phép điều gì?",
                    new string[]
                    {
                        "Mọi chương trình đều có quyền quản trị",
                        "Máy hoạt động mà không cần bộ nhớ",
                        "Nhiều tác vụ tiến triển nhờ lập lịch, chuyển đổi hoặc chạy trên nhiều bộ xử lý",
                        "Mọi tác vụ luôn thực thi đúng cùng một thời điểm trên một lõi"
                    },
                    2,
                    QuestionDifficulty.Medium),

                new Question(
                    "SYS-TB11",
                    "Cấp quyền tối thiểu cần thiết cho tài khoản giúp điều gì?",
                    new string[]
                    {
                        "Loại bỏ nhu cầu sao lưu",
                        "Bảo đảm ổ đĩa không bao giờ hỏng",
                        "Tăng dung lượng RAM vật lý",
                        "Giảm phạm vi thiệt hại khi tài khoản hoặc chương trình bị lạm dụng"
                    },
                    3,
                    QuestionDifficulty.Medium),

                new Question(
                    "SYS-TB12",
                    "Hệ thống tệp có vai trò gì trên thiết bị lưu trữ?",
                    new string[]
                    {
                        "Tổ chức tệp, thư mục và thông tin quản lý liên quan",
                        "Chỉ điều khiển tốc độ quạt",
                        "Chỉ định tuyến gói tin",
                        "Chỉ biến đổi điện áp nguồn"
                    },
                    0,
                    QuestionDifficulty.Medium),

                // KHÓ: 9 câu.
                new Question(
                    "SYS-K01",
                    "Một chương trình truy cập trang bộ nhớ ảo hợp lệ nhưng trang đó hiện chưa nằm trong RAM. Hệ điều hành thường phải làm gì trước khi tiếp tục?",
                    new string[]
                    {
                        "Đổi toàn bộ tên tệp trên máy",
                        "Nạp trang cần thiết vào RAM và cập nhật ánh xạ",
                        "Luôn xóa chương trình ngay lập tức",
                        "Bỏ qua mọi phép truy cập bộ nhớ"
                    },
                    1,
                    QuestionDifficulty.Hard),

                new Question(
                    "SYS-K02",
                    "Luồng A giữ khóa 1 và chờ khóa 2; luồng B giữ khóa 2 và chờ khóa 1. Nếu không bên nào tự nhả khóa hoặc hết thời gian chờ, tình trạng này là gì?",
                    new string[]
                    {
                        "Nén bộ nhớ",
                        "Phân giải tên miền",
                        "Bế tắc, tức các bên chờ nhau vô hạn",
                        "Sao lưu dữ liệu"
                    },
                    2,
                    QuestionDifficulty.Hard),

                new Question(
                    "SYS-K03",
                    "Biến đếm ban đầu là 0. Hai luồng đều đọc 0 trước khi mỗi luồng ghi lại giá trị đã tăng 1, không có đồng bộ. Giá trị cuối có thể là gì?",
                    new string[]
                    {
                        "2 một cách chắc chắn",
                        "3",
                        "4",
                        "1"
                    },
                    3,
                    QuestionDifficulty.Hard),

                new Question(
                    "SYS-K04",
                    "Theo đơn vị nhị phân, 4 GiB bằng bao nhiêu byte?",
                    new string[]
                    {
                        "4.294.967.296 byte",
                        "4.000.000.000 byte",
                        "4.194.304 byte",
                        "4.096 byte"
                    },
                    0,
                    QuestionDifficulty.Hard),

                new Question(
                    "SYS-K05",
                    "Một không gian địa chỉ có 32 bit và mỗi địa chỉ trỏ tới 1 byte có kích thước lý thuyết tối đa bao nhiêu?",
                    new string[]
                    {
                        "4 MiB",
                        "4 GiB",
                        "2 GiB",
                        "32 GiB"
                    },
                    1,
                    QuestionDifficulty.Hard),

                new Question(
                    "SYS-K06",
                    "Một tác vụ mất 60 giây tính toán và 40 giây chờ vào/ra, hai phần nối tiếp. Nếu chỉ phần tính toán nhanh gấp đôi, tổng thời gian mới là bao nhiêu?",
                    new string[]
                    {
                        "80 giây",
                        "100 giây",
                        "70 giây",
                        "50 giây"
                    },
                    2,
                    QuestionDifficulty.Hard),

                new Question(
                    "SYS-K07",
                    "Hai tiến trình P1 và P2 cùng sẵn sàng tại thời điểm 0, mỗi tiến trình cần 3 đơn vị CPU. Lập lịch luân phiên với lượt 2 đơn vị, P1 chạy trước, không có chi phí chuyển đổi hoặc chờ vào/ra. P1 hoàn tất lúc nào?",
                    new string[]
                    {
                        "Thời điểm 3",
                        "Thời điểm 4",
                        "Thời điểm 6",
                        "Thời điểm 5"
                    },
                    3,
                    QuestionDifficulty.Hard),

                new Question(
                    "SYS-K08",
                    "Kích thước trang là 4.096 byte, địa chỉ byte bắt đầu từ 0. Địa chỉ 8.197 có độ lệch trong trang bằng bao nhiêu?",
                    new string[]
                    {
                        "5 byte",
                        "2 byte",
                        "4.096 byte",
                        "8.192 byte"
                    },
                    0,
                    QuestionDifficulty.Hard),

                new Question(
                    "SYS-K09",
                    "Bộ nhớ đệm chứa tối đa 3 trang, ban đầu rỗng, dùng chính sách loại trang lâu nhất chưa được sử dụng. Với chuỗi truy cập 1, 2, 3, 1, 4 thì trang nào bị loại khi nạp 4?",
                    new string[]
                    {
                        "Trang 4",
                        "Trang 2",
                        "Trang 1",
                        "Trang 3"
                    },
                    1,
                    QuestionDifficulty.Hard),

            };
        }

    }
}
