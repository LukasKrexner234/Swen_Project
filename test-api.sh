#!/bin/bash

set -euo pipefail

BASE_URL="http://localhost:8080"
USERNAME="testuser_$(date +%s)_$$"
PASSWORD="test123456"

echo "=== MRP API TESTS ==="

# 1. Registrierung
echo
echo "1. REGISTER"

curl -fsS -X POST "$BASE_URL/api/users/register" \
  -H "Content-Type: application/json" \
  -d "{\"username\":\"$USERNAME\",\"password\":\"$PASSWORD\"}"

echo

# 2. Login
echo
echo "2. LOGIN"

LOGIN_RESPONSE=$(curl -fsS -X POST "$BASE_URL/api/users/login" \
  -H "Content-Type: application/json" \
  -d "{\"username\":\"$USERNAME\",\"password\":\"$PASSWORD\"}")

echo "$LOGIN_RESPONSE"

TOKEN=$(echo "$LOGIN_RESPONSE" | jq -r '.token')

if [[ -z "$TOKEN" || "$TOKEN" == "null" ]]; then
    echo "ERROR: No token received"
    exit 1
fi

# 3. CREATE
echo
echo "3. CREATE MEDIA"

CREATE_RESPONSE=$(curl -fsS -X POST "$BASE_URL/api/media" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "title": "Interstellar",
    "description": "Science-Fiction-Film",
    "mediaType": "Movie",
    "releaseYear": 2014,
    "genre": "Sci-Fi",
    "ageRestriction": 12
  }')

echo "$CREATE_RESPONSE"

MEDIA_ID=$(echo "$CREATE_RESPONSE" | jq -r '.id')

if [[ -z "$MEDIA_ID" || "$MEDIA_ID" == "null" ]]; then
    echo "ERROR: No media ID received"
    exit 1
fi

# 4. READ
echo
echo "4. READ MEDIA"

curl -fsS "$BASE_URL/api/media/$MEDIA_ID"
echo

# 5. UPDATE
echo
echo "5. UPDATE MEDIA"

curl -fsS -X PUT "$BASE_URL/api/media/$MEDIA_ID" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "title": "Interstellar Updated",
    "description": "Updated description",
    "mediaType": "Movie",
    "releaseYear": 2014,
    "genre": "Sci-Fi",
    "ageRestriction": 12
  }'

echo

# 6. DELETE
echo
echo "6. DELETE MEDIA"

DELETE_STATUS=$(curl -sS -o /dev/null -w "%{http_code}" \
  -X DELETE "$BASE_URL/api/media/$MEDIA_ID" \
  -H "Authorization: Bearer $TOKEN")

if [[ "$DELETE_STATUS" != "204" ]]; then
    echo "ERROR: Expected HTTP 204, got $DELETE_STATUS"
    exit 1
fi

echo "HTTP $DELETE_STATUS - Media deleted"

# ==========================================
# NEGATIVE TESTS
# ==========================================

echo
echo "=== NEGATIVE TESTS ==="

# Hilfsfunktion zum Überprüfen der HTTP-Statuscodes
check_status() {
    local expected=$1
    local actual=$2
    local test_name=$3

    if [[ "$actual" == "$expected" ]]; then
        echo "PASS: $test_name (HTTP $actual)"
    else
        echo "FAIL: $test_name (Expected $expected, got $actual)"
        exit 1
    fi
}


# 1. Login mit falschem Passwort
echo
echo "1. INVALID LOGIN"

STATUS=$(curl -sS -o /dev/null -w "%{http_code}" \
    -X POST "$BASE_URL/api/users/login" \
    -H "Content-Type: application/json" \
    -d "{\"username\":\"$USERNAME\",\"password\":\"wrongpassword\"}")

check_status 401 "$STATUS" "Login with wrong password"


# 2. Medium ohne Token erstellen
echo
echo "2. CREATE WITHOUT TOKEN"

STATUS=$(curl -sS -o /dev/null -w "%{http_code}" \
    -X POST "$BASE_URL/api/media" \
    -H "Content-Type: application/json" \
    -d '{"title":"Test","mediaType":"Movie","releaseYear":2020}')

check_status 401 "$STATUS" "Create media without token"


# 3. Fremdes Medium bearbeiten
echo
echo "3. UPDATE ANOTHER USER'S MEDIA"

# Zweiten Benutzer registrieren
USERNAME2="testuser2_$(date +%s)_$$"

curl -fsS -X POST "$BASE_URL/api/users/register" \
    -H "Content-Type: application/json" \
    -d "{\"username\":\"$USERNAME2\",\"password\":\"$PASSWORD\"}"

echo

# Zweiten Benutzer anmelden
LOGIN_RESPONSE2=$(curl -fsS -X POST "$BASE_URL/api/users/login" \
    -H "Content-Type: application/json" \
    -d "{\"username\":\"$USERNAME2\",\"password\":\"$PASSWORD\"}")

TOKEN2=$(echo "$LOGIN_RESPONSE2" | jq -r '.token')

if [[ -z "$TOKEN2" || "$TOKEN2" == "null" ]]; then
    echo "ERROR: No token received for second user"
    exit 1
fi

# Benutzer A erstellt ein Medium
CREATE_RESPONSE=$(curl -fsS -X POST "$BASE_URL/api/media" \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer $TOKEN" \
    -d '{
        "title": "Protected Movie",
        "description": "Test movie",
        "mediaType": "Movie",
        "releaseYear": 2020,
        "genre": "Drama",
        "ageRestriction": 12
    }')

MEDIA_ID=$(echo "$CREATE_RESPONSE" | jq -r '.id')

if [[ -z "$MEDIA_ID" || "$MEDIA_ID" == "null" ]]; then
    echo "ERROR: No media ID received"
    exit 1
fi

# Benutzer B versucht, das Medium zu bearbeiten
STATUS=$(curl -sS -o /dev/null -w "%{http_code}" \
    -X PUT "$BASE_URL/api/media/$MEDIA_ID" \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer $TOKEN2" \
    -d '{
        "title": "Unauthorized Update",
        "description": "Changed by another user",
        "mediaType": "Movie",
        "releaseYear": 2020,
        "genre": "Drama",
        "ageRestriction": 12
    }')

check_status 403 "$STATUS" "Update another user's media"


# 4. Medium mit leerem Titel erstellen
echo
echo "4. CREATE WITH EMPTY TITLE"

STATUS=$(curl -sS -o /dev/null -w "%{http_code}" \
    -X POST "$BASE_URL/api/media" \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer $TOKEN" \
    -d '{
        "title": "",
        "mediaType": "Movie",
        "releaseYear": 2020
    }')

check_status 400 "$STATUS" "Create media with empty title"


# Testmedium wieder löschen
echo
echo "CLEANUP"

STATUS=$(curl -sS -o /dev/null -w "%{http_code}" \
    -X DELETE "$BASE_URL/api/media/$MEDIA_ID" \
    -H "Authorization: Bearer $TOKEN")

check_status 204 "$STATUS" "Delete test media"

echo
echo "=== ALL NEGATIVE TESTS PASSED ==="
