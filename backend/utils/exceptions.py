class MaliciousContentError(Exception):
    """Raised when a malicious response is detected."""

    def __init__(self, message="Malicious response detected. Blocking output."):
        super().__init__(message)


class ModelResponseError(Exception):
    """Raised when errors occurs during model response/parse."""

    def __init__(self, message="Error generating response"):
        super().__init__(message)
