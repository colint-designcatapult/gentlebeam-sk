#!/usr/bin/env python3
"""
Add RELATIONS sections to test cases based on generated mappings.
"""
import json
import re
from pathlib import Path

def load_mappings(mapping_file):
    """Load TC to RQ mappings."""
    with open(mapping_file, 'r') as f:
        return json.load(f)

def load_file(filepath):
    """Load test protocol file."""
    with open(filepath, 'r') as f:
        return f.read()

def save_file(filepath, content):
    """Save test protocol file."""
    with open(filepath, 'w') as f:
        f.write(content)

def add_relations_to_file(content, mappings, doc_type):
    """Add RELATIONS sections to test cases in the document."""
    # Create a mapping of test_case_uid -> requirement_uids
    tc_to_rq = {item['test_case_uid']: item['requirement_uids'] for item in mappings}
    
    # Pattern to find test cases
    # Match [TEST_CASE] followed by UID, TITLE, and then sections until the next [TEST_CASE] or end
    pattern = r'(\[TEST_CASE\]\s*\nUID:\s*(\S+)\s*\n(?:.*?\n)*?(?=\n(?:\[TEST_CASE\]|$)))'
    
    def add_relations_to_match(match):
        test_case_block = match.group(1)
        test_case_uid = match.group(2)
        
        if test_case_uid not in tc_to_rq:
            return test_case_block
        
        requirement_uids = tc_to_rq[test_case_uid]
        
        # Check if RELATIONS already exists
        if 'RELATIONS:' in test_case_block:
            return test_case_block
        
        # Add RELATIONS before the next [TEST_CASE] or at the end
        relations_text = '\nRELATIONS:'
        for rq_uid in requirement_uids:
            relations_text += f'\n- TYPE: Parent\n  VALUE: {rq_uid}\n  ROLE: Verifies'
        
        # Insert RELATIONS at the end of this test case block
        return test_case_block.rstrip() + relations_text + '\n'
    
    # Use DOTALL to match across multiple lines
    modified = re.sub(pattern, add_relations_to_match, content, flags=re.DOTALL | re.MULTILINE)
    return modified

def main():
    mapping_file = '/home/runner/work/gentlebeam-sk/gentlebeam-sk/tc_rq_mappings.json'
    mappings = load_mappings(mapping_file)
    
    # Process external test protocol
    external_protocol_file = '/home/runner/work/gentlebeam-sk/gentlebeam-sk/cnc/voxelray-gentlebeamcnc/docs/external/main_test_protocol.sdoc'
    print(f"Processing {external_protocol_file}...")
    content = load_file(external_protocol_file)
    modified_content = add_relations_to_file(content, mappings['external'], 'external')
    save_file(external_protocol_file, modified_content)
    print(f"✓ Updated external test protocol")
    
    # Process internal test protocol
    internal_protocol_file = '/home/runner/work/gentlebeam-sk/gentlebeam-sk/cnc/voxelray-gentlebeamcnc/docs/internal/main_test_protocol.sdoc'
    print(f"Processing {internal_protocol_file}...")
    content = load_file(internal_protocol_file)
    modified_content = add_relations_to_file(content, mappings['internal'], 'internal')
    save_file(internal_protocol_file, modified_content)
    print(f"✓ Updated internal test protocol")

if __name__ == '__main__':
    main()
