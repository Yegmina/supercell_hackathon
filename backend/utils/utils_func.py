import json
import time
from .logger import logger
from flask import Response


def make_sse(data: dict) -> str:
    return f"data: {json.dumps(data)}\n\n"


def make_sse_response(data: dict) -> Response:
    return Response(make_sse(data), content_type="text/event-stream")


def exponential_retry(func, *args, match_exception_keywords: tuple | str = None, max_retries=5, initial_retry_delay=4,
                      retry_time_factor=2, **kwargs):
    """
    Retries the given function up to `max_retries` times with exponential backoff if an error occurs.
    If `retry_keywords` is None or an empty tuple, retries on all exceptions.
    """
    if isinstance(match_exception_keywords, str):
        match_exception_keywords = (match_exception_keywords,)  # convert to tuple if it's not

    delay = initial_retry_delay
    for attempt in range(max_retries):
        try:
            return func(*args, **kwargs)
        except Exception as e:
            error_msg = str(e)

            # if no keywords are provided, retry on all exceptions
            if not match_exception_keywords or any(keyword in error_msg for keyword in match_exception_keywords):
                logger.debug(f"⚠️ Received error '{error_msg}', retrying in {delay} seconds "
                             f"(attempt {attempt + 1}/{max_retries})")
                time.sleep(delay)
                delay *= retry_time_factor
            else:
                raise  # if keywords don't match, raise

    # when all retries fail
    raise Exception(f"❌ ERROR: Resource exhausted after {max_retries} retries.")
