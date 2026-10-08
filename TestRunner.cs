using System;
using System.Collections.Generic;

namespace AiLaTrieuPhu
{
    // Chạy kiểm thử bằng cách gọi trực tiếp các lớp xử lý.
    // Không phụ thuộc cửa sổ, nút bấm hoặc thành phần giao diện.
    public static class TestRunner
    {
        // Giá trị mong đợi được ghi riêng theo SPEC v2.
        // Không lấy từ GameRules để tránh dùng chính dữ liệu
        // cần kiểm tra làm đáp án cho bài kiểm thử.
        private static readonly int[] ExpectedPrizeLadder =
        {
            100,
            200,
            300,
            500,
            1000,
            2000,
            4000,
            8000,
            16000,
            25000
        };

        // Điểm gọi công khai của bộ kiểm thử.
        // Mỗi kịch bản luôn tạo đúng một dòng PASS hoặc FAIL.
        // Một kịch bản thất bại không ngăn các kịch bản sau chạy.
        public static List<string> RunAll()
        {
            List<string> results = new List<string>();

            RunCase(
                results,
                "01",
                "Đúng hết 10 câu → 25.000 USD",
                TestWinAllQuestions);

            RunCase(
                results,
                "02",
                "Sai ở câu 2 → 0 USD",
                () => TestWrongAnswer(2, 0, 202));

            RunCase(
                results,
                "03",
                "Sai ở câu 7 → 1.000 USD",
                () => TestWrongAnswer(7, 1000, 303));

            RunCase(
                results,
                "04",
                "Sai ở câu 10 → 1.000 USD",
                () => TestWrongAnswer(10, 1000, 404));

            RunCase(
                results,
                "05",
                "Dừng trước câu 8 → 4.000 USD",
                () => TestStopBeforeQuestion(8, 4000, 505));

            RunCase(
                results,
                "06",
                "Dừng trước câu 10 → 16.000 USD",
                () => TestStopBeforeQuestion(10, 16000, 606));

            RunCase(
                results,
                "07",
                "Hai ván liên tiếp không trùng câu, trạng thái sạch",
                TestTwoConsecutiveGames);

            return results;
        }

        // Bao từng kịch bản trong một vùng xử lý lỗi riêng.
        // Nếu kiểm tra không đạt hoặc chương trình phát sinh lỗi,
        // ghi thông tin vào kết quả thay vì mở hộp thoại.
        private static void RunCase(
            List<string> results,
            string number,
            string description,
            Action test)
        {
            try
            {
                test();

                results.Add(
                    "PASS | " + number + " | " + description);
            }
            catch (Exception ex)
            {
                // Giữ thông báo trên một dòng để dễ hiển thị.
                string errorMessage = ex.Message
                    .Replace("\r", " ")
                    .Replace("\n", " ");

                results.Add(
                    "FAIL | " + number + " | " + description +
                    " | Lỗi: " + errorMessage);
            }
        }

        // Kịch bản 1: trả lời đúng toàn bộ 10 câu.
        private static void TestWinAllQuestions()
        {
            Dictionary<string, Question> originals;

            GameRules game = CreateGame(101, out originals);

            PlayWinningGame(game, originals, null);
        }

        // Kịch bản 2, 3 và 4 dùng chung cách chạy:
        // trả lời đúng các câu trước, rồi cố ý chọn sai ở câu đích.
        private static void TestWrongAnswer(
            int questionNumber,
            int expectedFinalMoney,
            int randomSeed)
        {
            Dictionary<string, Question> originals;

            GameRules game = CreateGame(randomSeed, out originals);

            ReachQuestion(game, originals, questionNumber);

            Question question = game.CurrentQuestion;

            // Tìm đáp án đúng từ nội dung gốc, sau đó chọn
            // một vị trí khác để bảo đảm đây là đáp án sai.
            int correctIndex =
                FindExpectedCorrectIndex(question, originals);

            int wrongIndex = (correctIndex + 1) % 4;

            AnswerResult result =
                game.SubmitAnswer(question.Id, wrongIndex);

            Require(
                result == AnswerResult.Incorrect,
                "Chọn đáp án sai nhưng không nhận kết quả Incorrect.");

            Require(
                game.CorrectCount == questionNumber - 1,
                "Trả lời sai đã làm thay đổi số câu trả lời đúng.");

            Require(
                game.CurrentQuestionNumber == questionNumber,
                "Trả lời sai nhưng chương trình vẫn chuyển câu.");

            // Tiền hiện tại phản ánh những câu đã trả lời đúng,
            // còn tiền kết thúc phải phản ánh mốc an toàn.
            Require(
                game.CurrentMoney ==
                    ExpectedMoneyForCorrectCount(questionNumber - 1),
                "CurrentMoney không khớp số câu đã trả lời đúng.");

            VerifyFinished(
                game,
                GameState.Lost,
                expectedFinalMoney);
        }

        // Kịch bản 5 và 6:
        // đi tới câu đích nhưng chưa trả lời câu đó, rồi dừng.
        private static void TestStopBeforeQuestion(
            int questionNumber,
            int expectedFinalMoney,
            int randomSeed)
        {
            Dictionary<string, Question> originals;

            GameRules game = CreateGame(randomSeed, out originals);

            ReachQuestion(game, originals, questionNumber);

            bool stopped = game.StopGame(game.CurrentQuestion.Id);

            Require(
                stopped,
                "Không dừng được khi câu đang chờ trả lời.");

            Require(
                game.CorrectCount == questionNumber - 1,
                "Dừng chơi đã làm thay đổi số câu trả lời đúng.");

            Require(
                game.CurrentQuestionNumber == questionNumber,
                "Dừng chơi nhưng chương trình vẫn chuyển câu.");

            Require(
                game.CurrentMoney == expectedFinalMoney,
                "CurrentMoney tại thời điểm dừng không đúng.");

            VerifyFinished(
                game,
                GameState.Stopped,
                expectedFinalMoney);
        }

        // Kịch bản 7:
        // dùng cùng một đối tượng GameRules để chơi hai ván.
        // Ván đầu kết thúc rồi mới gọi bắt đầu ván mới.
        private static void TestTwoConsecutiveGames()
        {
            Dictionary<string, Question> originals;

            GameRules game = CreateGame(707, out originals);

            // Chơi hết ván đầu để thu thập đủ 10 mã câu.
            // Các hàm trợ giúp cũng được gọi trong quá trình chơi.
            HashSet<string> firstGameIds =
                PlayWinningGame(game, originals, null);

            game.StartNewGame();

            // Xác minh tiền, tiến độ và quyền thao tác được đặt lại.
            Require(
                game.State == GameState.Playing,
                "Ván mới vẫn giữ trạng thái kết thúc của ván cũ.");

            Require(
                game.CurrentQuestionNumber == 1,
                "Ván mới không bắt đầu từ câu 1.");

            Require(
                game.CorrectCount == 0,
                "Ván mới chưa xóa số câu đúng của ván cũ.");

            Require(
                game.CurrentMoney == 0,
                "Ván mới chưa đưa tiền hiện tại về 0.");

            Require(
                game.FinalMoney == null,
                "Ván mới vẫn giữ tiền kết thúc của ván cũ.");

            Require(
                game.CanAnswer && game.CanStop,
                "Ván mới chưa mở lại quyền trả lời và dừng.");

            // Trong ván thứ hai, kiểm tra từng mã câu
            // đều không thuộc danh sách 10 câu của ván thứ nhất.
            HashSet<string> secondGameIds =
                PlayWinningGame(game, originals, firstGameIds);

            Require(
                firstGameIds.Count == 10 &&
                secondGameIds.Count == 10,
                "Mỗi ván phải có đúng 10 mã câu khác nhau.");

            foreach (string id in secondGameIds)
            {
                Require(
                    !firstGameIds.Contains(id),
                    "Hai ván liên tiếp bị trùng câu: " + id);
            }
        }

        // Tạo một bộ ngân hàng và luật riêng cho mỗi kịch bản.
        // Đồng thời giữ câu hỏi gốc để kiểm tra việc trộn đáp án.
        private static GameRules CreateGame(
            int randomSeed,
            out Dictionary<string, Question> originals)
        {
            QuestionBank bank = new QuestionBank(randomSeed);

            originals = new Dictionary<string, Question>(
                StringComparer.OrdinalIgnoreCase);

            int easyCount = 0;
            int mediumCount = 0;
            int hardCount = 0;

            foreach (Question question in bank.AllQuestions)
            {
                Require(
                    !originals.ContainsKey(question.Id),
                    "Ngân hàng bị trùng mã: " + question.Id);

                originals.Add(question.Id, question);

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
                }
            }

            Require(
                easyCount >= 9,
                "Ngân hàng có ít hơn 9 câu dễ.");

            Require(
                mediumCount >= 12,
                "Ngân hàng có ít hơn 12 câu trung bình.");

            Require(
                hardCount >= 9,
                "Ngân hàng có ít hơn 9 câu khó.");

            GameRules game = new GameRules(bank);

            // Kiểm tra cả thang tiền công khai mà giao diện sẽ đọc.
            Require(
                game.PrizeLadder.Count == ExpectedPrizeLadder.Length,
                "Thang tiền không có đúng 10 bậc.");

            for (int i = 0; i < ExpectedPrizeLadder.Length; i++)
            {
                Require(
                    game.PrizeLadder[i] == ExpectedPrizeLadder[i],
                    "Sai thang tiền ở câu " + (i + 1) + ".");
            }

            return game;
        }

        // Chơi thắng một ván và trả về toàn bộ mã đã gặp.
        // forbiddenIds được dùng khi cần tránh các mã của ván trước.
        private static HashSet<string> PlayWinningGame(
            GameRules game,
            Dictionary<string, Question> originals,
            HashSet<string> forbiddenIds)
        {
            HashSet<string> seenIds =
                new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int number = 1; number <= 10; number++)
            {
                CheckPendingQuestion(game, originals, number);

                string currentId = game.CurrentQuestion.Id;

                Require(
                    seenIds.Add(currentId),
                    "Một ván bị lặp câu: " + currentId);

                if (forbiddenIds != null)
                {
                    Require(
                        !forbiddenIds.Contains(currentId),
                        "Ván mới chứa câu của ván trước: " + currentId);
                }

                AnswerCorrectlyAndAdvance(game, originals);
            }

            Require(
                seenIds.Count == 10,
                "Ván thắng không chứa đủ 10 câu khác nhau.");

            Require(
                game.CorrectCount == 10,
                "Thắng ván nhưng số câu đúng không bằng 10.");

            Require(
                game.CurrentQuestionNumber == 10,
                "Sau câu cuối, chương trình đã chuyển sang câu 11.");

            Require(
                game.CurrentMoney == 25000,
                "Đúng đủ 10 câu nhưng CurrentMoney không bằng 25000.");

            VerifyFinished(game, GameState.Won, 25000);

            return seenIds;
        }

        // Đưa ván từ câu 1 tới câu đích.
        // Khi hàm kết thúc, câu đích vẫn chưa được trả lời.
        private static void ReachQuestion(
            GameRules game,
            Dictionary<string, Question> originals,
            int targetQuestionNumber)
        {
            Require(
                targetQuestionNumber >= 1 &&
                targetQuestionNumber <= 10,
                "Số câu đích của kiểm thử phải từ 1 đến 10.");

            HashSet<string> seenIds =
                new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int number = 1; number <= targetQuestionNumber; number++)
            {
                CheckPendingQuestion(game, originals, number);

                Require(
                    seenIds.Add(game.CurrentQuestion.Id),
                    "Một ván bị lặp câu: " + game.CurrentQuestion.Id);

                // Giữ nguyên câu đích để bên gọi chọn sai hoặc dừng.
                if (number == targetQuestionNumber)
                {
                    return;
                }

                AnswerCorrectlyAndAdvance(game, originals);
            }
        }

        // Kiểm tra các điều kiện tại điểm quyết định:
        // câu đang hiển thị và chưa được trả lời.
        private static void CheckPendingQuestion(
            GameRules game,
            Dictionary<string, Question> originals,
            int expectedNumber)
        {
            Require(
                game.State == GameState.Playing,
                "Ván đã kết thúc trước khi tới câu " +
                expectedNumber + ".");

            Require(
                game.CurrentQuestionNumber == expectedNumber,
                "Số thứ tự câu hiện tại không đúng.");

            Require(
                game.CorrectCount == expectedNumber - 1,
                "Số câu đúng không khớp tiến độ.");

            Require(
                game.CurrentMoney ==
                    ExpectedMoneyForCorrectCount(expectedNumber - 1),
                "Tiền hiện tại không khớp tiến độ.");

            Require(
                game.FinalMoney == null,
                "Ván đang chơi nhưng đã có tiền kết thúc.");

            Require(
                game.CanAnswer && game.CanStop,
                "Câu chưa trả lời nhưng bị khóa quyền trả lời hoặc dừng.");

            Question question = game.CurrentQuestion;

            // Kiểm tra tầng theo vị trí, không theo dữ liệu của engine.
            QuestionDifficulty expectedDifficulty;

            if (expectedNumber <= 3)
            {
                expectedDifficulty = QuestionDifficulty.Easy;
            }
            else if (expectedNumber <= 7)
            {
                expectedDifficulty = QuestionDifficulty.Medium;
            }
            else
            {
                expectedDifficulty = QuestionDifficulty.Hard;
            }

            Require(
                question.Difficulty == expectedDifficulty,
                "Sai tầng độ khó ở câu " + expectedNumber + ".");

            // Kiểm tra đáp án sau trộn bằng nội dung đúng của câu gốc.
            int expectedCorrectIndex =
                FindExpectedCorrectIndex(question, originals);

            Require(
                question.CorrectAnswerIndex == expectedCorrectIndex,
                "Trộn đáp án nhưng cập nhật sai CorrectAnswerIndex: " +
                question.Id);

            // Kiểm tra hai loại dữ liệu trợ giúp.
            CheckAssistance(game, expectedCorrectIndex);

            // Các thao tác sai đầu vào phải bị từ chối,
            // không được làm thua hoặc thay đổi tiến độ.
            Require(
                game.SubmitAnswer(question.Id, -1) == AnswerResult.Invalid,
                "Chương trình chấp nhận chỉ số đáp án -1.");

            Require(
                game.SubmitAnswer(question.Id, 4) == AnswerResult.Invalid,
                "Chương trình chấp nhận chỉ số đáp án 4.");

            string wrongQuestionId = question.Id + "_KHONG_KHOP";

            Require(
                game.SubmitAnswer(wrongQuestionId, expectedCorrectIndex) ==
                    AnswerResult.Invalid,
                "Chương trình nhận câu trả lời có mã câu không khớp.");

            Require(
                !game.StopGame(wrongQuestionId),
                "Chương trình cho dừng với mã câu không khớp.");

            Require(
                !game.MoveToNextQuestion(),
                "Chương trình cho chuyển câu trước khi trả lời.");

            // Xác minh các yêu cầu bị từ chối không để lại thay đổi.
            Require(
                game.State == GameState.Playing &&
                game.CurrentQuestionNumber == expectedNumber &&
                game.CurrentQuestion.Id == question.Id &&
                game.CorrectCount == expectedNumber - 1 &&
                game.CurrentMoney ==
                    ExpectedMoneyForCorrectCount(expectedNumber - 1) &&
                game.FinalMoney == null &&
                game.CanAnswer &&
                game.CanStop,
                "Yêu cầu không hợp lệ đã làm thay đổi ván.");
        }

        // Tìm vị trí đúng dựa trên câu gốc.
        // Đồng thời kiểm tra việc trộn không làm mất, thêm
        // hoặc thay nội dung của bốn lựa chọn.
        private static int FindExpectedCorrectIndex(
            Question current,
            Dictionary<string, Question> originals)
        {
            Question original;

            Require(
                originals.TryGetValue(current.Id, out original),
                "Câu đang chơi không tồn tại trong ngân hàng: " +
                current.Id);

            Require(
                current.Text == original.Text,
                "Nội dung câu hỏi bị thay đổi sau khi chọn.");

            Require(
                current.Difficulty == original.Difficulty,
                "Tầng độ khó bị thay đổi sau khi chọn.");

            string[] currentOptions = current.Options;

            HashSet<string> originalOptions = new HashSet<string>(
                original.Options,
                StringComparer.Ordinal);

            Require(
                currentOptions.Length == 4,
                "Câu đang chơi không có đúng 4 lựa chọn.");

            HashSet<string> seenOptions =
                new HashSet<string>(StringComparer.Ordinal);

            string correctText = original.GetCorrectAnswerText();
            int expectedIndex = -1;

            for (int i = 0; i < currentOptions.Length; i++)
            {
                Require(
                    originalOptions.Contains(currentOptions[i]),
                    "Sau khi trộn xuất hiện đáp án không có trong câu gốc.");

                Require(
                    seenOptions.Add(currentOptions[i]),
                    "Sau khi trộn có đáp án bị lặp.");

                if (string.Equals(
                    currentOptions[i],
                    correctText,
                    StringComparison.Ordinal))
                {
                    expectedIndex = i;
                }
            }

            Require(
                expectedIndex >= 0,
                "Nội dung đáp án đúng bị mất sau khi trộn.");

            return expectedIndex;
        }

        // Kiểm tra gợi ý đúng và quyền 50:50 tại câu chưa trả lời.
        private static void CheckAssistance(
            GameRules game,
            int expectedCorrectIndex)
        {
            int? suggestedIndex = game.GetCorrectAnswerIndex();

            Require(
                suggestedIndex.HasValue &&
                suggestedIndex.Value == expectedCorrectIndex,
                "Hàm gợi ý không trả đúng vị trí đáp án đúng.");

            int[] hiddenIndexes = game.GetTwoWrongAnswerIndexes();

            Require(
                hiddenIndexes != null && hiddenIndexes.Length == 2,
                "50:50 không trả đúng hai vị trí.");

            Require(
                hiddenIndexes[0] != hiddenIndexes[1],
                "50:50 trả hai vị trí trùng nhau.");

            for (int i = 0; i < hiddenIndexes.Length; i++)
            {
                Require(
                    hiddenIndexes[i] >= 0 && hiddenIndexes[i] <= 3,
                    "50:50 trả chỉ số nằm ngoài 0–3.");

                Require(
                    hiddenIndexes[i] != expectedCorrectIndex,
                    "50:50 đã loại đáp án đúng.");
            }

            // Ghi lại kết quả trước khi cố ý sửa mảng nhận được.
            int firstIndex = hiddenIndexes[0];
            int secondIndex = hiddenIndexes[1];

            hiddenIndexes[0] = -99;

            int[] repeatedIndexes = game.GetTwoWrongAnswerIndexes();

            Require(
                repeatedIndexes.Length == 2 &&
                repeatedIndexes[0] == firstIndex &&
                repeatedIndexes[1] == secondIndex,
                "Kết quả 50:50 bị đổi khi gọi lại hoặc bị sửa từ bên ngoài.");
        }

        // Trả lời đúng câu hiện tại, kiểm tra thời điểm chờ chuyển,
        // rồi chuyển câu nếu đây chưa phải câu cuối.
        private static void AnswerCorrectlyAndAdvance(
            GameRules game,
            Dictionary<string, Question> originals)
        {
            Question question = game.CurrentQuestion;
            int number = game.CurrentQuestionNumber;

            int correctIndex =
                FindExpectedCorrectIndex(question, originals);

            AnswerResult result =
                game.SubmitAnswer(question.Id, correctIndex);

            Require(
                result == AnswerResult.Correct,
                "Đáp án đúng theo câu gốc bị chấm sai.");

            Require(
                game.CorrectCount == number,
                "Sau khi trả lời đúng, số câu đúng không tăng chính xác.");

            Require(
                game.CurrentMoney == ExpectedPrizeLadder[number - 1],
                "Tiền sau khi trả lời đúng không khớp thang tiền.");

            Require(
                game.CurrentQuestionNumber == number &&
                game.CurrentQuestion.Id == question.Id,
                "SubmitAnswer tự chuyển câu trước khi được yêu cầu.");

            // Sau khi chọn đáp án, không được dừng hoặc chọn lần hai.
            Require(
                !game.CanAnswer && !game.CanStop,
                "Sau khi trả lời vẫn còn quyền trả lời hoặc dừng.");

            Require(
                !game.StopGame(question.Id),
                "Chương trình cho dừng trong lúc chờ chuyển câu.");

            Require(
                game.SubmitAnswer(question.Id, correctIndex) ==
                    AnswerResult.Invalid,
                "Chương trình chấp nhận trả lời lặp một câu.");

            Require(
                game.GetCorrectAnswerIndex() == null &&
                game.GetTwoWrongAnswerIndexes().Length == 0,
                "Vẫn cung cấp trợ giúp sau khi câu đã được trả lời.");

            if (number == 10)
            {
                Require(
                    game.State == GameState.Won,
                    "Đúng câu 10 nhưng ván chưa chuyển sang thắng.");

                Require(
                    game.FinalMoney == 25000,
                    "Đúng câu 10 nhưng tiền kết thúc không bằng 25000.");

                Require(
                    !game.MoveToNextQuestion(),
                    "Chương trình cho chuyển sang câu 11.");

                return;
            }

            Require(
                game.State == GameState.Playing &&
                game.FinalMoney == null,
                "Ván kết thúc quá sớm sau một câu trả lời đúng.");

            Require(
                game.MoveToNextQuestion(),
                "Không chuyển được câu sau khi trả lời đúng.");

            Require(
                game.CurrentQuestionNumber == number + 1,
                "Chuyển câu không tăng đúng một vị trí.");

            Require(
                game.CanAnswer && game.CanStop,
                "Câu mới chưa mở quyền trả lời và dừng.");

            // Một lần gọi chuyển dư không được bỏ qua câu mới.
            Require(
                !game.MoveToNextQuestion(),
                "Gọi chuyển câu lần hai đã bỏ qua câu chưa trả lời.");

            // Thao tác còn sót lại từ câu cũ phải bị từ chối.
            Require(
                game.SubmitAnswer(question.Id, correctIndex) ==
                    AnswerResult.Invalid,
                "Câu mới nhận nhầm thao tác trả lời của câu cũ.");

            Require(
                !game.StopGame(question.Id),
                "Câu mới nhận nhầm thao tác dừng của câu cũ.");

            Require(
                game.State == GameState.Playing &&
                game.CurrentQuestionNumber == number + 1 &&
                game.CorrectCount == number &&
                game.CurrentMoney == ExpectedPrizeLadder[number - 1] &&
                game.FinalMoney == null &&
                game.CanAnswer &&
                game.CanStop,
                "Thao tác thừa hoặc thao tác cũ đã làm thay đổi ván.");
        }

        // Kiểm tra trạng thái và số tiền khi kết thúc.
        // Sau đó thử thao tác thêm để bảo đảm kết quả đã được khóa.
        private static void VerifyFinished(
            GameRules game,
            GameState expectedState,
            int expectedFinalMoney)
        {
            Require(
                game.State == expectedState,
                "Trạng thái kết thúc không đúng.");

            string actualMoney = game.FinalMoney.HasValue
                ? game.FinalMoney.Value.ToString()
                : "chưa có";

            Require(
                game.FinalMoney.HasValue &&
                game.FinalMoney.Value == expectedFinalMoney,
                "Tiền kết thúc phải là " + expectedFinalMoney +
                " USD nhưng thực tế là " + actualMoney + ".");

            Require(
                !game.CanAnswer && !game.CanStop,
                "Ván kết thúc nhưng vẫn cho trả lời hoặc dừng.");

            int savedCorrectCount = game.CorrectCount;
            int savedCurrentMoney = game.CurrentMoney;
            int savedQuestionNumber = game.CurrentQuestionNumber;
            string savedQuestionId = game.CurrentQuestion.Id;

            Require(
                game.SubmitAnswer(savedQuestionId, 0) ==
                    AnswerResult.Invalid,
                "Ván kết thúc nhưng vẫn nhận đáp án.");

            Require(
                !game.StopGame(savedQuestionId),
                "Ván kết thúc nhưng vẫn nhận lệnh dừng.");

            Require(
                !game.MoveToNextQuestion(),
                "Ván kết thúc nhưng vẫn cho chuyển câu.");

            Require(
                game.GetCorrectAnswerIndex() == null,
                "Ván kết thúc nhưng vẫn cung cấp gợi ý đáp án.");

            Require(
                game.GetTwoWrongAnswerIndexes().Length == 0,
                "Ván kết thúc nhưng vẫn cung cấp dữ liệu 50:50.");

            Require(
                game.State == expectedState &&
                game.FinalMoney == expectedFinalMoney &&
                game.CorrectCount == savedCorrectCount &&
                game.CurrentMoney == savedCurrentMoney &&
                game.CurrentQuestionNumber == savedQuestionNumber &&
                game.CurrentQuestion.Id == savedQuestionId &&
                !game.CanAnswer &&
                !game.CanStop,
                "Thao tác sau khi kết thúc đã làm thay đổi kết quả.");
        }

        // Tra số tiền mong đợi theo số câu đã đúng.
        // Hàm này dùng thang tiền độc lập của bộ kiểm thử.
        private static int ExpectedMoneyForCorrectCount(int correctCount)
        {
            Require(
                correctCount >= 0 && correctCount <= 10,
                "Số câu đúng phải nằm trong khoảng 0–10.");

            if (correctCount == 0)
            {
                return 0;
            }

            return ExpectedPrizeLadder[correctCount - 1];
        }

        // Khi một điều kiện không đạt, dừng kịch bản hiện tại
        // và chuyển mô tả lỗi cho RunCase tạo dòng FAIL.
        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}