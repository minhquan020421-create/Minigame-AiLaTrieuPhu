using System;
using System.Collections.Generic;

namespace AiLaTrieuPhu
{
    // Bốn trạng thái của một ván chơi.
    public enum GameState
    {
        Playing,    // Đang chơi.
        Won,        // Đã trả lời đúng cả 10 câu.
        Lost,       // Đã kết thúc vì trả lời sai.
        Stopped     // Đã chủ động dừng chơi.
    }

    // Kết quả của một yêu cầu trả lời.
    // Yêu cầu không hợp lệ không bị tính là trả lời sai.
    public enum AnswerResult
    {
        Invalid,    // Yêu cầu không hợp lệ, không thay đổi ván.
        Correct,    // Đáp án đúng.
        Incorrect   // Đáp án sai, kết thúc ván.
    }

    // Quản lý luật và trạng thái ván chơi.
    // Không tham chiếu bất kỳ thành phần giao diện nào.
    public class GameRules
    {
        // Ngân hàng được dùng lại giữa các ván.
        private readonly QuestionBank _questionBank;

        // Bộ tạo số ngẫu nhiên dùng để chọn hai đáp án sai.
        private readonly Random _random;

        // Thang tiền SPEC v2, đơn vị USD.
        // Vị trí 0 tương ứng câu 1, vị trí 9 tương ứng câu 10.
        private readonly int[] _prizeLadder =
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

        // Toàn bộ 10 câu đã được chọn cho ván hiện tại.
        // Khi bắt đầu ván tiếp theo, dùng mã của cả danh sách
        // này để tránh lặp, kể cả câu chưa kịp hiển thị.
        private IReadOnlyList<Question> _gameQuestions;

        // Vị trí câu hiện tại trong danh sách, bắt đầu từ 0.
        private int _currentQuestionIndex;

        // Ghi nhận câu hiện tại đã được trả lời hay chưa.
        // Sau khi trả lời đúng, cờ này vẫn giữ nguyên cho đến
        // khi giao diện gọi chuyển sang câu tiếp theo.
        private bool _currentQuestionAnswered;

        // Giữ kết quả 50:50 của câu hiện tại.
        // Gọi nhiều lần trong cùng câu vẫn nhận cùng hai vị trí.
        private int[] _cachedFiftyFiftyIndexes;

        // Trạng thái chỉ được thay đổi từ bên trong lớp.
        public GameState State { get; private set; }

        // Số câu đã trả lời đúng trong ván hiện tại.
        public int CorrectCount { get; private set; }

        // Tiền thực nhận khi kết thúc ván.
        // int? nghĩa là có thể chứa số nguyên hoặc chưa có giá trị.
        // Khi đang chơi, thuộc tính này có giá trị null.
        public int? FinalMoney { get; private set; }

        // Câu hỏi hiện tại với bốn lựa chọn đã được trộn.
        // Khi ván kết thúc, vẫn giữ câu cuối đang hiển thị
        // để giao diện có thể trình bày kết quả.
        public Question CurrentQuestion
        {
            get
            {
                return _gameQuestions[_currentQuestionIndex];
            }
        }

        // Số thứ tự dành cho người chơi, bắt đầu từ 1.
        public int CurrentQuestionNumber
        {
            get
            {
                return _currentQuestionIndex + 1;
            }
        }

        // Tiền ứng với số câu đã trả lời đúng.
        // Đây không nhất thiết là tiền thực nhận nếu trả lời sai.
        // Khi kết thúc, bảng thành tích phải lấy FinalMoney.
        public int CurrentMoney
        {
            get
            {
                if (CorrectCount == 0)
                {
                    return 0;
                }

                return _prizeLadder[CorrectCount - 1];
            }
        }

        // Cung cấp thang tiền chỉ đọc để giao diện có thể hiển thị.
        public IReadOnlyList<int> PrizeLadder
        {
            get
            {
                return Array.AsReadOnly(_prizeLadder);
            }
        }

        // Chỉ được trả lời khi ván còn đang chơi
        // và câu hiện tại chưa được trả lời.
        public bool CanAnswer
        {
            get
            {
                return State == GameState.Playing &&
                    !_currentQuestionAnswered;
            }
        }

        // Điều kiện dừng giống điều kiện được trả lời:
        // câu đang hiển thị và chưa chọn đáp án.
        public bool CanStop
        {
            get
            {
                return State == GameState.Playing &&
                    !_currentQuestionAnswered;
            }
        }

        // Cách tạo thông thường.
        // Tự tạo ngân hàng và bắt đầu ván đầu tiên.
        public GameRules()
            : this(new QuestionBank())
        {
        }

        // Cho phép cung cấp ngân hàng từ bên ngoài.
        // TestRunner có thể dùng ngân hàng với số khởi đầu cố định
        // để tái hiện quá trình chọn câu khi cần kiểm tra.
        public GameRules(QuestionBank questionBank)
        {
            if (questionBank == null)
            {
                throw new ArgumentNullException(
                    nameof(questionBank),
                    "Ngân hàng câu hỏi không được thiếu.");
            }

            _questionBank = questionBank;
            _random = new Random();

            // Chưa có ván trước trong lần khởi tạo đầu tiên.
            _gameQuestions = new List<Question>().AsReadOnly();

            StartNewGame();
        }

        // Bắt đầu hoặc chơi lại.
        // Chỉ giữ mã câu của ván liền trước để loại khỏi ván mới.
        // Tiến độ, kết quả và dữ liệu trợ giúp của câu đều được xóa.
        public void StartNewGame()
        {
            List<string> previousQuestionIds = new List<string>();

            foreach (Question question in _gameQuestions)
            {
                previousQuestionIds.Add(question.Id);
            }

            // Chuẩn bị danh sách mới trước khi thay đổi ván hiện tại.
            // Nếu ngân hàng báo lỗi, ván hiện tại chưa bị xóa dở.
            IReadOnlyList<Question> newQuestions =
                _questionBank.CreateGameQuestions(previousQuestionIds);

            if (newQuestions.Count != QuestionBank.QuestionsPerGame)
            {
                throw new InvalidOperationException(
                    "Một ván phải có đúng 10 câu hỏi.");
            }

            // Áp dụng dữ liệu mới và đặt lại toàn bộ tiến độ.
            _gameQuestions = newQuestions;
            _currentQuestionIndex = 0;
            _currentQuestionAnswered = false;
            _cachedFiftyFiftyIndexes = null;

            CorrectCount = 0;
            FinalMoney = null;
            State = GameState.Playing;
        }

        // Nhận câu trả lời cho đúng câu đang hiển thị.
        //
        // questionId: mã câu mà giao diện đang cho người chơi xem.
        // answerIndex: vị trí lựa chọn, từ 0 đến 3.
        //
        // Mã câu giúp từ chối thao tác còn sót lại từ câu trước.
        // Mọi yêu cầu không hợp lệ đều không thay đổi trạng thái.
        public AnswerResult SubmitAnswer(
            string questionId,
            int answerIndex)
        {
            // Không nhận thêm câu trả lời khi đã trả lời
            // hoặc khi ván đã kết thúc.
            if (!CanAnswer)
            {
                return AnswerResult.Invalid;
            }

            // Chỉ chấp nhận thao tác thuộc câu hiện tại.
            if (!IsCurrentQuestionId(questionId))
            {
                return AnswerResult.Invalid;
            }

            // Chỉ số ngoài 0–3 là yêu cầu không hợp lệ.
            // Không được xử lý trường hợp này thành thua.
            if (answerIndex < 0 || answerIndex > 3)
            {
                return AnswerResult.Invalid;
            }

            // Từ thời điểm nhận một đáp án hợp lệ,
            // khóa cả trả lời lặp và dừng ở câu hiện tại.
            _currentQuestionAnswered = true;

            // Sai: giữ nguyên số câu đã đúng và chốt tiền an toàn.
            if (answerIndex != CurrentQuestion.CorrectAnswerIndex)
            {
                State = GameState.Lost;
                FinalMoney = CalculateFinalMoney();

                return AnswerResult.Incorrect;
            }

            // Đúng: tăng số câu đã hoàn thành.
            CorrectCount++;

            // Đúng câu 10 là thắng ngay, không có câu 11.
            if (CorrectCount == QuestionBank.QuestionsPerGame)
            {
                State = GameState.Won;
                FinalMoney = CalculateFinalMoney();
            }

            // Nếu chưa thắng, vẫn giữ nguyên câu đang hiển thị.
            // Giao diện gọi MoveToNextQuestion sau khi trình bày kết quả.
            return AnswerResult.Correct;
        }

        // Chuyển sang câu tiếp theo.
        // Chỉ thành công sau khi câu hiện tại đã được trả lời đúng
        // và ván vẫn còn tiếp tục.
        public bool MoveToNextQuestion()
        {
            if (State != GameState.Playing)
            {
                return false;
            }

            if (!_currentQuestionAnswered)
            {
                return false;
            }

            // Khi vừa trả lời đúng câu hiện tại,
            // số câu đã đúng phải bằng số thứ tự câu đó.
            if (CorrectCount != CurrentQuestionNumber)
            {
                return false;
            }

            // Không cho vượt khỏi danh sách 10 câu.
            if (_currentQuestionIndex >= _gameQuestions.Count - 1)
            {
                return false;
            }

            // Chuyển câu và mở lại quyền trả lời, quyền dừng.
            _currentQuestionIndex++;
            _currentQuestionAnswered = false;
            _cachedFiftyFiftyIndexes = null;

            return true;
        }

        // Dừng tại câu đang hiển thị nhưng chưa trả lời.
        // Nhận tiền của câu đã trả lời đúng gần nhất.
        // Dừng trước câu 1 nhận 0 USD.
        public bool StopGame(string questionId)
        {
            if (!CanStop)
            {
                return false;
            }

            if (!IsCurrentQuestionId(questionId))
            {
                return false;
            }

            State = GameState.Stopped;
            FinalMoney = CalculateFinalMoney();

            return true;
        }

        // Trả về hai vị trí đáp án sai khác nhau cho quyền 50:50.
        // Hàm chỉ cung cấp dữ liệu, không quản lý số lần dùng quyền.
        // Phần trợ giúp của nhóm chịu trách nhiệm quản lý lượt dùng.
        public int[] GetTwoWrongAnswerIndexes()
        {
            // Khi không còn ở điểm được trả lời,
            // không cung cấp dữ liệu trợ giúp mới.
            if (!CanAnswer)
            {
                return new int[0];
            }

            // Giữ cùng kết quả khi được gọi lại trong một câu.
            if (_cachedFiftyFiftyIndexes != null)
            {
                return (int[])_cachedFiftyFiftyIndexes.Clone();
            }

            List<int> wrongIndexes = new List<int>();

            // Thu thập đúng ba vị trí sai của câu hiện tại.
            for (int i = 0; i < 4; i++)
            {
                if (i != CurrentQuestion.CorrectAnswerIndex)
                {
                    wrongIndexes.Add(i);
                }
            }

            // Trộn ba vị trí sai rồi lấy hai vị trí đầu.
            for (int i = wrongIndexes.Count - 1; i > 0; i--)
            {
                int randomIndex = _random.Next(i + 1);

                int temporary = wrongIndexes[i];
                wrongIndexes[i] = wrongIndexes[randomIndex];
                wrongIndexes[randomIndex] = temporary;
            }

            _cachedFiftyFiftyIndexes = new int[]
            {
                wrongIndexes[0],
                wrongIndexes[1]
            };

            // Trả bản sao để bên nhận không sửa kết quả đã lưu.
            return (int[])_cachedFiftyFiftyIndexes.Clone();
        }

        // Cung cấp vị trí đúng cho phần khán giả hoặc gọi điện.
        // Cách dựng tỷ lệ, lời thoại và lượt sử dụng thuộc phần trợ giúp.
        public int? GetCorrectAnswerIndex()
        {
            if (!CanAnswer)
            {
                return null;
            }

            return CurrentQuestion.CorrectAnswerIndex;
        }

        // Kiểm tra mã của thao tác có khớp câu đang hiển thị không.
        // Không phân biệt chữ hoa và chữ thường trong mã.
        private bool IsCurrentQuestionId(string questionId)
        {
            if (string.IsNullOrWhiteSpace(questionId))
            {
                return false;
            }

            return string.Equals(
                questionId.Trim(),
                CurrentQuestion.Id,
                StringComparison.OrdinalIgnoreCase);
        }

        // Tính tiền thực nhận theo đúng ba kiểu kết thúc.
        // Dùng số câu đã trả lời đúng để tránh công nhận mốc quá sớm.
        private int CalculateFinalMoney()
        {
            switch (State)
            {
                case GameState.Won:
                    // Đúng đủ 10 câu: nhận 25.000 USD.
                    return _prizeLadder[9];

                case GameState.Stopped:
                    // Dừng: nhận tiền ứng với số câu đã đúng.
                    // Nếu chưa đúng câu nào, CurrentMoney bằng 0.
                    return CurrentMoney;

                case GameState.Lost:
                    // Mốc câu 10 chỉ đạt khi đã đúng đủ 10 câu.
                    // Trong luồng hiện tại, lúc đó ván đã thắng,
                    // nên không thể tiếp tục trả lời sai.
                    if (CorrectCount >= 10)
                    {
                        return _prizeLadder[9];
                    }

                    // Đã đúng câu 5 thì giữ được 1.000 USD.
                    // Đang hiển thị câu 5 nhưng chưa đúng câu đó
                    // thì CorrectCount mới bằng 4, chưa đạt mốc.
                    if (CorrectCount >= 5)
                    {
                        return _prizeLadder[4];
                    }

                    // Chưa vượt qua mốc nào.
                    return 0;

                default:
                    // Không được chốt tiền khi ván vẫn đang chơi.
                    throw new InvalidOperationException(
                        "Chỉ được tính tiền kết thúc khi ván đã kết thúc.");
            }
        }
    }
}